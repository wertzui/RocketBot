﻿using System;
using System.Globalization;
using System.Windows.Data;

namespace PokemonGo.WPF.Converters
{
    public class TeamToGymImagePathConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var teamName = (string)value;
            if(teamName != null)
                return $"pack://siteoforigin:,,,/Images/Map/gymLogo_{teamName.ToLowerInvariant()}.png";
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}