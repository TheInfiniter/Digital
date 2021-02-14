using Harmonic.Modulation;
using OxyPlot;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows.Threading;

namespace Harmonic
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        private bool _start = false;
        private string _step;

        #region таймеры
        private DispatcherTimer _newTimer;
        private int _tick;
        private double _interval;
        private bool reset = false;
        #endregion

        #region параметры несущего сигнала
        private double _freq;
        private double _period;
        #endregion

        private int _invalidate;
        private int _amount;

        #region свойства
        public double Frequency
        {
            get => _freq;
            set 
            {
                _freq = value;
                OnPropertyChanged(nameof(Frequency));
            }
        }

        public double Period
        {
            get => _period;
            set
            {
                _period = value;
                OnPropertyChanged(nameof(Period));
            }
        }

        public double Interval
        {
            get => _interval;
            set
            {
                _interval = value;

                if (_newTimer != null)
                {
                    _newTimer.Interval = TimeSpan.FromMilliseconds(value);
                }

                OnPropertyChanged(nameof(Interval));
            }
        }

        public int Invalidate
        {
            get => _invalidate;
            set
            {
                _invalidate = value;
                OnPropertyChanged(nameof(Invalidate));
            }
        }

        public int Amount
        {
            get => _amount;
            set
            {
                _amount = value;
                OnPropertyChanged(nameof(Amount));
            }
        }

        public string Step
        {
            get => _step;
            set
            {
                _step = value;
                OnPropertyChanged(nameof(Step));
            }
        }

        public string StartName => Start ? "Остановить" : "Запустить";

        public bool Start
        {
            get => _start;
            set
            {
                _newTimer.IsEnabled = _start = value;
                OnPropertyChanged(nameof(Start));
                OnPropertyChanged(nameof(StartName));
            }
        }
        #endregion

        public List<DataPoint> Points { get; set; }
        public List<DataPoint> FSK { get; set; }

        public ICommand Generate { get; set; }

        #region модуляции
        MainSignal signal;
        FSK fsk;
        #endregion

        public MainViewModel()
        {
            Frequency = 10;
            Period = 400;

            Amount = 250;

            Invalidate = 0;

            Interval = 40;
            _newTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(Interval)
            };

            _newTimer.Tick += OnTimedEvent;
            Step = $"Шаг алгоритма: {_tick} ";

            Points = new List<DataPoint>();

            signal = new MainSignal();
            fsk = new FSK();

            Generate = new RelayCommand(o =>
            {
                SetTimer();
            });
        }

        private void SetTimer()
        {
            if (!reset)
            {
                reset = !reset;
                for (int i = 0; i < Amount; i++)
                {
                    Points.Add(signal.GeneratePoint(_freq, _period));
                }
                Invalidate++;
            }

            Start = !Start;
        }

        private void OnTimedEvent(object source, EventArgs e)
        {
            Points.Add(signal.GeneratePoint(Frequency, Period));

            if (Points.Count > Amount)
            {
                for (int i = 0; i < (Points.Count - Amount); i++)
                {
                    Points.RemoveAt(i);
                }
            }

            Invalidate++;
            _tick++;
            Step = $"Шаг алгоритма: {_tick} ";
        }
    }
}
