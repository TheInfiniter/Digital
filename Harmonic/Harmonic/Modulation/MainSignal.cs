﻿using OxyPlot;
using System;
using System.Collections.Generic;

namespace Harmonic.Modulation
{
    public class MainSignal
    {
        private double _phase;
        private int _currentX;

        public MainSignal()
        {
            _phase = 0;
            _currentX = 0;
        }

        /// <summary>
        /// Сгенерировать точку графика несущего сигнала.
        /// </summary>
        /// <param name="freq">Частота.</param>
        /// <param name="period">Период дискретизации.</param>
        /// <returns>Точка несущего сигнала.</returns>
        public DataPoint GeneratePoint(double freq, double period)
        {   
            DataPoint point = new DataPoint(_currentX, Math.Sin(_phase));
            _phase += 2 * Math.PI * freq / period;
            _phase %= 2 * Math.PI;

            _currentX++;

            return point;
        }
    }
}
