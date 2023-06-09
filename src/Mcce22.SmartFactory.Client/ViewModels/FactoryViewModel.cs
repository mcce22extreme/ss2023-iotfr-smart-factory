using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mcce22.SmartFactory.Client.Devices;
using Mcce22.SmartFactory.Client.Services;
using Newtonsoft.Json;

namespace Mcce22.SmartFactory.Client.ViewModels
{
    public partial class FactoryViewModel : ObservableObject
    {
        private readonly IMqttService _mqttService;

        private readonly List<IDevice> _devices = new List<IDevice>();

        public event EventHandler FactoryReseted;

        public event EventHandler<DeviceChangedEventArgs> DeviceChanged;

        public B1Device B1 { get; }

        public B2Device B2 { get; }

        public B3Device B3 { get; }

        public B4Device B4 { get; }

        public B5Device B5 { get; }

        public E2Device E2 { get; }

        public F1Device F1 { get; }

        public F2Device F2 { get; }

        public S1Device S1 { get; }

        public S2Device S2 { get; }

        public S3Device S3 { get; }

        public S9Device S9 { get; set; }

        public S10Device S10 { get; set; }

        public S11Device S11 { get; set; }

        public S14Device S14 { get; }

        public S15Device S15 { get; }

        public S21Device S21 { get; }

        public S22Device S22 { get; }

        public S28Device S28 { get; set; }

        public Q1Device Q1 { get; }

        public Q2Device Q2 { get; }

        public Q3Device Q3 { get; }

        public Q4Device Q4 { get; }

        public Q11Device Q11 { get; }

        [ObservableProperty]
        private bool _lightOn;

        [ObservableProperty]
        private string _messageLog;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(StartFactoryCommand))]
        [NotifyCanExecuteChangedFor(nameof(ResetFactoryCommand))]
        private bool _factoryStarted;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(StartFactoryCommand))]
        [NotifyCanExecuteChangedFor(nameof(ResetFactoryCommand))]
        private bool _factoryStarting;

        public FactoryViewModel(IMqttService mqttService)
        {
            _mqttService = mqttService;
            _mqttService.MessageReceived += OnMqttMessageReceived;

            _devices.Add(B1 = new B1Device(mqttService));
            _devices.Add(B2 = new B2Device(mqttService));
            _devices.Add(B3 = new B3Device(mqttService));
            _devices.Add(B4 = new B4Device(mqttService));
            _devices.Add(B5 = new B5Device(mqttService));
            _devices.Add(E2 = new E2Device(mqttService));
            _devices.Add(F1 = new F1Device(mqttService));
            _devices.Add(F2 = new F2Device(mqttService));
            _devices.Add(S1 = new S1Device(mqttService));
            _devices.Add(S2 = new S2Device(mqttService));
            _devices.Add(S3 = new S3Device(mqttService));
            _devices.Add(S9 = new S9Device(mqttService));
            _devices.Add(S10 = new S10Device(mqttService));
            _devices.Add(S11 = new S11Device(mqttService));
            _devices.Add(S14 = new S14Device(mqttService));
            _devices.Add(S15 = new S15Device(mqttService));
            _devices.Add(S21 = new S21Device(mqttService));
            _devices.Add(S22 = new S22Device(mqttService));
            _devices.Add(S28 = new S28Device(mqttService));
            _devices.Add(Q1 = new Q1Device(mqttService));
            _devices.Add(Q2 = new Q2Device(mqttService));
            _devices.Add(Q3 = new Q3Device(mqttService));
            _devices.Add(Q4 = new Q4Device(mqttService));
            _devices.Add(Q11 = new Q11Device(mqttService));

            foreach (var device in _devices)
            {
                device.PropertyChanged += OnDeviceChanged;
            }
        }

        private Task InitializeFactory()
        {
            foreach (var device in _devices)
            {
                device.Reset();
            }

            FactoryReseted?.Invoke(this, EventArgs.Empty);

            return Task.CompletedTask;
        }

        private void OnDeviceChanged(object sender, PropertyChangedEventArgs e)
        {
            DeviceChanged?.Invoke(this, new DeviceChangedEventArgs(sender as IDevice, e.PropertyName));
        }

        [RelayCommand]
        private void ClearMessageLog()
        {
            MessageLog = string.Empty;
        }

        [RelayCommand(CanExecute = nameof(CanStartFactory))]
        private async void StartFactory()
        {
            if (CanStartFactory())
            {
                FactoryStarting = true;

                await Task.Delay(200);

                await _mqttService.Connect();

                await InitializeFactory();

                FactoryStarted = true;
                FactoryStarting = false;
            }
        }

        private bool CanStartFactory()
        {
            return !FactoryStarting && !FactoryStarted;
        }

        [RelayCommand(CanExecute = nameof(CanResetFactory))]
        private async void ResetFactory()
        {
            if (CanResetFactory())
            {
                await InitializeFactory();
            }
        }

        private bool CanResetFactory()
        {
            return FactoryStarted || FactoryStarting;
        }

        private void OnMqttMessageReceived(object sender, MessageReceivedArgs e)
        {
            MessageLog += $"{DateTime.Now}: {JsonConvert.SerializeObject(e.Message)}" + Environment.NewLine;
        }
    }

    public class DeviceChangedEventArgs : EventArgs
    {
        public IDevice Device { get; }

        public string PropertyName { get; }

        public DeviceChangedEventArgs(IDevice device, string propertyName)
        {
            Device = device;
            PropertyName = propertyName;
        }
    }
}
