using Harmonic.Modulation;
using OxyPlot;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
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
        private DispatcherTimer _timer;
        private int _tick;
        private double _interval;
        private bool reset = false;
        #endregion

        #region параметры несущего сигнала
        private double _freqCarrier;
        private double _periodCarrier;
        #endregion

        #region параметры модулирующего сигнала
        private double _freqModulate;
        private double _periodModulate;
        #endregion

        #region прочие параметры
        private int _invalidate;
        private int _amount;
        #endregion

        #region свойства несущего сигнала
        public double FrequencyCarrier
        {
            get => _freqCarrier;
            set 
            {
                _freqCarrier = value;
                OnPropertyChanged(nameof(FrequencyCarrier));
            }
        }

        public double PeriodCarrier
        {
            get => _periodCarrier;
            set
            {
                _periodCarrier = value;
                OnPropertyChanged(nameof(PeriodCarrier));
            }
        }
        #endregion

        #region свойства модулирующего сигнала
        public double FrequencyModulate
        {
            get => _freqModulate;
            set
            {
                _freqModulate = value;
                OnPropertyChanged(nameof(FrequencyModulate));
            }
        }

        public double PeriodModulate
        {
            get => _periodModulate;
            set
            {
                _periodModulate = value;
                OnPropertyChanged(nameof(PeriodModulate));
            }
        }
        #endregion

        #region прочие свойства
        public double Interval
        {
            get => _interval;
            set
            {
                _interval = value;

                if (_timer != null)
                {
                    _timer.Interval = TimeSpan.FromMilliseconds(value);
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
                _timer.IsEnabled = _start = value;
                OnPropertyChanged(nameof(Start));
                OnPropertyChanged(nameof(StartName));
            }
        }
        #endregion

        public List<DataPoint> PointsModulate { get; set; }
        public List<DataPoint> PointsCarrier { get; set; }
        public List<DataPoint> PointsASK { get; set; }

        public ICommand Generate { get; set; }

        #region сигналы
        SignalCarrier carrier;
        SignalModulate modulate;
        ASK ask;
        #endregion

        public MainViewModel()
        {
            FrequencyCarrier = 40;
            PeriodCarrier = 400;

            FrequencyModulate = 10;
            PeriodModulate = 800;

            Amount = 250;

            Invalidate = 0;

            Interval = 40;
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(Interval)
            };

            _timer.Tick += OnTimedEvent;
            Step = $"Шаг алгоритма: {_tick} ";

            PointsCarrier = new List<DataPoint>();
            PointsModulate = new List<DataPoint>();
            PointsASK = new List<DataPoint>();

            carrier = new SignalCarrier();
            modulate = new SignalModulate();
            ask = new ASK();

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
                    PointsCarrier.Add(carrier.GeneratePoint(FrequencyCarrier, PeriodCarrier));
                    PointsModulate.Add(modulate.GeneratePoint(FrequencyModulate, PeriodModulate));
                    PointsASK.Add(ask.GeneratePoint(modulate.Phase, carrier.Phase));
                }
                Invalidate++;
            }

            Start = !Start;
        }

        private void OnTimedEvent(object source, EventArgs e)
        {
            GenerateCarrier();
            GenerateModul();
            GenerateModulated();

            Invalidate++;
            _tick++;
            Step = $"Шаг алгоритма: {_tick} ";
        }

        private void GenerateCarrier()
        {
            PointsCarrier.Add(carrier.GeneratePoint(FrequencyCarrier, PeriodCarrier));

            if (PointsCarrier.Count > Amount)
            {
                for (int i = 0; i < (PointsCarrier.Count - Amount); i++)
                {
                    PointsCarrier.RemoveAt(i);
                }
            }
        }

        private void GenerateModul()
        {
            PointsModulate.Add(modulate.GeneratePoint(FrequencyModulate, PeriodModulate));

            if (PointsModulate.Count > Amount)
            {
                for (int i = 0; i < (PointsModulate.Count - Amount); i++)
                {
                    PointsModulate.RemoveAt(i);
                }
            }
        }

        private void GenerateModulated()
        {
            PointsASK.Add(ask.GeneratePoint(modulate.Phase, carrier.Phase));

            if (PointsASK.Count > Amount)
            {
                for (int i = 0; i < (PointsASK.Count - Amount); i++)
                {
                    PointsASK.RemoveAt(i);
                }
            }
        }
    }
}
