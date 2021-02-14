using OxyPlot;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Timers;
using System.Windows.Input;

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

        private Timer _timer;
        private int _tick;
        private double _interval;
        private bool reset = false;

        private double _freq;
        private double _period;

        private int _invalidate;
        private double _phase;
        private int _currentX;

        private int _amount;

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

                if (_timer != null)
                {
                    _timer.Interval = value;
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
                _timer.Enabled = _start = value;
                OnPropertyChanged(nameof(Start));
                OnPropertyChanged(nameof(StartName));
            }
        }

        public List<DataPoint> Points { get; set; }

        public ICommand Generate { get; set; }

        public MainViewModel()
        {
            Frequency = 10;
            Period = 400;

            Amount = 150;

            Invalidate = 0;

            _phase = 0;

            Interval = 40;
            _timer = new Timer(Interval);
            _timer.Elapsed += OnTimedEvent;
            _tick = 0;
            _timer.AutoReset = true;
            Step = $"Шаг алгоритма: {_tick} ";

            Points = new List<DataPoint>();

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
                Points.Clear();
                _currentX = 0;

                for (int i = 0; i < Amount; i++)
                {
                    Points.Add(new DataPoint(_currentX, Math.Sin(_phase)));
                    _phase += 2 * Math.PI * Frequency / Period;

                    if (_phase >= 2 * Math.PI)
                    {
                        _phase = 0;
                    }
                    _currentX++;
                }
                Invalidate++;
            }

            Start = !Start;
        }

        private void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            Points.Add(new DataPoint(_currentX, Math.Sin(_phase)));
            _phase += 2 * Math.PI * Frequency / Period;
            _phase %= (2 * Math.PI);

            if (Points.Count > Amount) Points.RemoveRange(0, 1);

            Invalidate++;
            _tick++;
            _currentX++;
            Step = $"Шаг алгоритма: {_tick} ";
        }
    }
}
