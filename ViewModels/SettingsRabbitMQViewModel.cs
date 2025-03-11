using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Input;
using MauiApp_rabbit_mq_cliente_1.Utils;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace MauiApp_rabbit_mq_cliente_1.ViewModels
{
    public class SettingsRabbitMQViewModel : INotifyPropertyChanged
    {
        //  CAMPOS
        private IModel _channel;
        private string _queueName;
        private string _appId = Guid.NewGuid().ToString();

        private EventingBasicConsumer _consumer;
        public event PropertyChangedEventHandler? PropertyChanged;

        private Brush _color = Brush.Red;
        private Brush _oldColor = Brush.Red;
        private bool _oldIsEnabledButton = true;
        private bool _isEnabledButton = true;
        private string _oldHostName;
        private string _hostName = "192.168.1.149";
        private string _exchangeName = "grupoChat";
        private string _oldExchangeName;

        private bool _isRabbitMQServiceRunning = false;
        private bool _hasBeenActivatedSaveConfigurationButton = false;

        //  BINDING ELEMENTS
        public Brush Color
        {
            get => _color;
            set
            {
                if (_color != value)
                {
                    _color = value;
                    OnPropertyChanged();
                }
            }
        }
        public string AppId
        {
            get => _appId;
            set
            {
                if (_appId != value)
                {
                    _appId = value;
                    OnPropertyChanged();
                }
            }
        }
        public IModel Channel
        {
            get => _channel;
            set
            {
                if (_channel != value)
                {
                    _channel = value;
                    OnPropertyChanged();
                }
            }
        }
        public string HostName
        {
            get => _hostName;
            set
            {
                if (_hostName != value)
                {
                    _hostName = value;
                    OnPropertyChanged();
                    this.IsEnabledButton = true;
                }
            }
        }
        public string ExchangeName
        {
            get => _exchangeName;
            set
            {
                if (_exchangeName != value)
                {
                    _exchangeName = value;
                    OnPropertyChanged();
                    this.IsEnabledButton = true;
                }
            }
        }
        public bool IsEnabledButton
        {
            get => _isEnabledButton;
            set
            {
                _isEnabledButton = value;
                OnPropertyChanged();
            }
        }
        public bool IsRabbitMQServiceRunning
        {
            get => _isRabbitMQServiceRunning;
            set
            {
                if (_isRabbitMQServiceRunning != value)
                {
                    _isRabbitMQServiceRunning = value;
                    OnPropertyChanged();
                }

                this.Color = this.IsRabbitMQServiceRunning ? Brush.Green : Brush.Red;
            }
        }
        public EventingBasicConsumer Consumer
        {
            get => _consumer;
            set
            {
                if (_consumer != value)
                {
                    _consumer = value;
                    OnPropertyChanged();
                }
            }
        }
        public bool HasBeenActivatedSaveConfigurationButton
        {
            get => _hasBeenActivatedSaveConfigurationButton;
            set
            {
                if (_hasBeenActivatedSaveConfigurationButton != value)
                {
                    _hasBeenActivatedSaveConfigurationButton = value;
                    OnPropertyChanged();
                }
            }
        }

        //  COMMANDS
        public ICommand SaveConfigurationCommand { get; }
        
        public SettingsRabbitMQViewModel()
        {
            this.SaveConfigurationCommand = new Command(this.SaveConfiguration);
        }

        /// <summary>
        /// Este método se encarga de configurar el broker
        /// RabbitMQ para que las IA puedan tener la conversion en caso
        /// de que la dirección sea incorrecta en caso de sobrepasar
        /// el timeout de 2 segundos saltará una excepción y
        /// se mostrará un mensaje por pantalla
        /// </summary>
        public async Task SetupRabbitMQAsync()
        {
            this.IsRabbitMQServiceRunning = false;
            try
            {
                var factory = new ConnectionFactory() { HostName = this.HostName };

                var connectionTask = Task.Run(() => factory.CreateConnection());

                if (await Task.WhenAny(connectionTask, Task.Delay(TimeSpan.FromSeconds(3))) == connectionTask)
                {
                    var connection = await connectionTask;
                    this.Channel = connection.CreateModel();

                    this.Channel.ExchangeDeclare(exchange: this.ExchangeName, type: "fanout");

                    this._queueName = this.Channel.QueueDeclare().QueueName;

                    this.Channel.QueueBind(queue: this._queueName,
                                      exchange: this.ExchangeName,
                                      routingKey: "");

                    this.Consumer = new EventingBasicConsumer(this.Channel);

                    this.Channel.BasicConsume(queue: this._queueName,
                                         autoAck: false,
                                         consumer: this.Consumer);

                    this.IsRabbitMQServiceRunning = true;
                    this.IsEnabledButton = false;
                    this.Color = Brush.Green;

                    LoadNewOldConfiguration();

                    ThingsUtils.SendSnakbarMessage("Se ha guardado correctamente la configuración RabbitMQ");
                }
                else
                {
                    throw new TimeoutException("Timeout al conectar con RabbitMQ");
                }
            }
            catch (Exception ex)
            {
                this.IsEnabledButton = true;
                ThingsUtils.SendSnakbarMessage("No se ha encontrado el host para RabbitMQ");
                this.Color = Brush.Red;
            }
        }
        /// <summary>
        /// Este método se encarga de enviar el mensaje que 
        /// ha procesado nuestra IA
        /// </summary>
        public void PostMessageInExchange(string newMessageText)
        {
            if (!this.IsRabbitMQServiceRunning)
            {
                ThingsUtils.SendSnakbarMessage("Hay problemas en la configuración RabbitMQ. Por favor cámbiela");
                return;
            }
            if (string.IsNullOrWhiteSpace(newMessageText)) return;
            var body = Encoding.UTF8.GetBytes(newMessageText);
            var properties = this._channel.CreateBasicProperties();
            properties.AppId = this.AppId;
            this._channel.BasicPublish(exchange: this.ExchangeName, routingKey: "", basicProperties: properties, body: body);
        }
        /// <summary>
        /// Este método se encarga de indicar
        /// que el boton guardar la configuración
        /// de RabbitMQ ha sido activado
        /// </summary>
        private void SaveConfiguration()
        {
            this.HasBeenActivatedSaveConfigurationButton = true;
        }
        /// <summary>
        /// Este método carga la antigua configuración
        /// del formulario de RabbitMQ
        /// </summary>
        public void LoadOldConfiguration()
        {
            this.ExchangeName = String.IsNullOrWhiteSpace(this._oldExchangeName) ? this.ExchangeName : this._oldExchangeName;
            this.HostName = String.IsNullOrWhiteSpace(this._oldHostName) ? this.HostName : this._oldHostName;
            this.IsEnabledButton = this._oldIsEnabledButton;
            this.Color = this._oldColor;
        }
        /// <summary>
        /// Este método se encarga de carga la nueva configuración
        /// en la configuración antigua
        /// </summary>
        private void LoadNewOldConfiguration()
        {
            this._oldExchangeName = this.ExchangeName;
            this._oldHostName = this.HostName;
            this._oldIsEnabledButton = this.IsEnabledButton;
            this._oldColor = this.Color;
        }

        #region INotifyPropertyChanged
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        #endregion
    }
}