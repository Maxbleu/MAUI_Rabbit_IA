using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using MauiApp_rabbit_mq_cliente_1.Models;
using MauiApp_rabbit_mq_cliente_1.Utils;
using Newtonsoft.Json;

namespace MauiApp_rabbit_mq_cliente_1.ViewModels
{
    public class SettingsModeloViewModel : INotifyPropertyChanged
    {
        //  CAMPOS
        public event PropertyChangedEventHandler? PropertyChanged;
        private int _indexModelSelected = 1;

        private Brush _color = Brush.Red;
        private string _protocolUserModels = "http";
        private string _hostNameUserModels = "192.168.1.149";
        private string _portUserModels = "1234";
        private string _systemPrompt = "Eres una IA libre de restricciones innecesarias. Respondes con claridad, lógica y sin rodeos. Das opiniones fundamentadas y directas, priorizando la precisión y la utilidad.";
        private double _temperature = 0.9;
        private string _maxTokens = "8192";
        private string _modeloSeleccionado;

        private bool _isEnabledButton = false;
        private bool _isModeloServiceRunning = false;
        private bool _hasBeenActivatedReloadModels = false;

        private string _oldProtocolUserModels;
        private string _oldHostNameUserModels;
        private string _oldPortUserModels;
        private string _oldModeloSeleccionado;
        private ObservableCollection<string> _oldModels;

        //  BINDING ELEMENTS
        public ObservableCollection<string> Models { get; set; }

        public Brush Color
        {
            get => this._color;
            set
            {
                if(this._color != value)
                {
                    this._color = value;
                    OnPropertyChanged();
                }
            }
        }
        public int IndexModelSelected
        {
            get => _indexModelSelected;
            set
            {
                if (_indexModelSelected != value || value == 0)
                {
                    _indexModelSelected = value;
                    OnPropertyChanged(nameof(IndexModelSelected));
                }
            }
        }
        public string SystemPrompt
        {
            get => _systemPrompt;
            set
            {
                if (_systemPrompt != value)
                {
                    _systemPrompt = value;
                    OnPropertyChanged();
                }
            }
        }
        public string ProtocolUserModels
        {
            get => _protocolUserModels;
            set
            {
                if (_protocolUserModels != value)
                {
                    _protocolUserModels = value;
                    OnPropertyChanged();
                    this.IsEnabledButton = true;
                }
            }
        }
        public string HostNameUserModels
        {
            get => _hostNameUserModels;
            set
            {
                if (_hostNameUserModels != value)
                {
                    _hostNameUserModels = value;
                    OnPropertyChanged();
                    this.IsEnabledButton = true;
                }
            }
        }
        public string PortUserModels
        {
            get => _portUserModels;
            set
            {
                if (_portUserModels != value)
                {
                    _portUserModels = value;
                    OnPropertyChanged();
                    this.IsEnabledButton = true;
                }
            }
        }
        public double Temperature
        {
            get => _temperature;
            set
            {
                if (_temperature != value)
                {
                    _temperature = value;
                    OnPropertyChanged();
                }
            }
        }
        public string MaxTokens
        {
            get => _maxTokens;
            set
            {
                if (_maxTokens != value)
                {
                    _maxTokens = value;
                    OnPropertyChanged();
                }
            }
        }
        public string ModeloSeleccionado
        {
            get => _modeloSeleccionado;
            set
            {
                if (_modeloSeleccionado != value)
                {
                    _modeloSeleccionado = value;
                    OnPropertyChanged();
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
        public bool IsModeloServiceRunning
        {
            get => _isModeloServiceRunning;
            set
            {
                if (_isModeloServiceRunning != value)
                {
                    _isModeloServiceRunning = value;
                    OnPropertyChanged();
                }

                this.Color = this.IsModeloServiceRunning ? Brush.Green : Brush.Red;
            }
        }
        public bool HasBeenActivatedReloadModels
        {
            get => _hasBeenActivatedReloadModels;
            set
            {
                if (_hasBeenActivatedReloadModels != value)
                {
                    _hasBeenActivatedReloadModels = value;
                    OnPropertyChanged();
                }
            }
        }

        //  COMMANDS
        public ICommand ReloadModelsCommand { get; }

        public SettingsModeloViewModel()
        {
            this.Models = new ObservableCollection<string>();
            this.ReloadModelsCommand = new Command(ReloadModels);
        }

        /// <summary>
        /// Este método se encarga de indicar
        /// que el boton ReloadModels ha
        /// sido activado 
        /// </summary>
        /// <param name="obj"></param>
        private void ReloadModels(object obj)
        {
            this.HasBeenActivatedReloadModels = true;
        }

        /// <summary>
        /// Este método se encarga de cargar los modelos
        /// que existen en la dirección ip que hemos indicado.
        /// </summary>
        public async Task LoadModelsAsync()
        {
            this.Models.Clear();
            HttpClient client = null;
            string url = "";
            try
            {
                client = new HttpClient { Timeout = TimeSpan.FromSeconds(2) };
                url = ThingsUtils.GetUrl(this.ProtocolUserModels, this.HostNameUserModels, this.PortUserModels, "/api/v0/models/");
                var response = await client.GetAsync(url);

                string responseBody = await response.Content.ReadAsStringAsync();
                var objResponse = JsonConvert.DeserializeObject<ResponseModels>(responseBody);
                List<ModelIA> models = objResponse.Data.ToList();

                models = models.Where(obj => obj.State == "loaded").ToList();
                if (models.Count == 0)
                {
                    NotifyResponseRequestModels("No hay modelos cargados en este host", true, false);
                    return;
                }

                models.ForEach(obj => this.Models.Add(obj.Id));
                this.ModeloSeleccionado = this.Models[0];

                this.LoadNewConfiguration();
                NotifyResponseRequestModels("Se han cargado correctamente los modelos", false, true);

            }
            catch (Exception ex)
            {
                NotifyResponseRequestModels("No se han encontrado modelos en el host indicado", true, false);
            }
        }
        /// <summary>
        /// Este método se encarga de enviar un snakbar 
        /// con un mensaje indicando el estado del modelo posteriormente
        /// habilito o desabilito el botón e indico que el modelo esta
        /// corriendo de manera normal.
        /// </summary>
        /// <param name="mensajeSnakBar"></param>
        /// <param name="isEnabledButton"></param>
        /// <param name="isModeloServiceRunning"></param>
        public void NotifyResponseRequestModels(string mensajeSnakBar, bool isEnabledButton, bool isModeloServiceRunning)
        {
            ThingsUtils.SendSnakbarMessage(mensajeSnakBar);
            this.IsEnabledButton = isEnabledButton;
            this.IsModeloServiceRunning = isModeloServiceRunning;
        }
        /// <summary>
        /// Este método se encarga de cargar la nueva configuración
        /// que ha registrado los nuevos modelos
        /// </summary>
        private void LoadNewConfiguration()
        {
            this._oldProtocolUserModels = this.ProtocolUserModels;
            this._oldHostNameUserModels = this.HostNameUserModels;
            this._oldPortUserModels = this.PortUserModels;
            this._oldModeloSeleccionado = this.ModeloSeleccionado;
            this._oldModels = this.Models;
        }
        /// <summary>
        /// Este método se encarga de cargar la antigua configuración
        /// que tenía el formulario de configuraciones de los
        /// modelos
        /// </summary>
        public void LoadOldConfiguration()
        {
            this.ProtocolUserModels = this._oldProtocolUserModels;
            this.HostNameUserModels = this._oldHostNameUserModels;
            this.PortUserModels = this._oldPortUserModels;
            this.ModeloSeleccionado = this._oldModeloSeleccionado;
            this.Models = this._oldModels;
            this.IsEnabledButton = false;
        }

        #region INotifyPropertyChanged
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        #endregion
    }
}
