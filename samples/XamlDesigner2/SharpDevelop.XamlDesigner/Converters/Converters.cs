﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Globalization;
using System.Windows;
using System.Collections;

namespace SharpDevelop.XamlDesigner.Converters
{
	public class IntFromEnumConverter : IValueConverter
	{
		public static IntFromEnumConverter Instance = new IntFromEnumConverter();

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return (int)value;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return Enum.ToObject(targetType, (int)value);
		}
	}

	public class HiddenWhenFalse : IValueConverter
	{
		public static HiddenWhenFalse Instance = new HiddenWhenFalse();

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return (bool)value ? Visibility.Visible : Visibility.Hidden;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}

	public class CollapsedWhenFalse : IValueConverter
	{
		public static CollapsedWhenFalse Instance = new CollapsedWhenFalse();

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return (bool)value ? Visibility.Visible : Visibility.Collapsed;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}

	public class LevelConverter : IValueConverter
	{
		public static LevelConverter Instance = new LevelConverter();

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return new Thickness(2 + 14 * (int)value, 0, 0, 0);
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}

	public class CollapsedWhenZero : IValueConverter
	{
		public static CollapsedWhenZero Instance = new CollapsedWhenZero();

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value == null || (int)value == 0) {
				return Visibility.Collapsed;
			}
			return Visibility.Visible;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}

	public class FalseWhenNull : IValueConverter
	{
		public static FalseWhenNull Instance = new FalseWhenNull();

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return value != null;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}

	public class BoldWhenTrue : IValueConverter
	{
		public static BoldWhenTrue Instance = new BoldWhenTrue();

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return (bool)value ? FontWeights.Bold : FontWeights.Normal;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}

	// Boxed int throw exception without converter (wpf bug?)
	public class SafeDoubleConverter : IValueConverter
	{
		public static SafeDoubleConverter Instance = new SafeDoubleConverter();

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return value ?? 0.0;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return value;
		}
	}
	public class HiddenWhenTrue : IValueConverter
	{
		public static HiddenWhenTrue Instance = new HiddenWhenTrue();

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return (bool)value ? Visibility.Hidden : Visibility.Visible;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}

	public class FullTypeNameConverter : IValueConverter
	{
		public static FullTypeNameConverter Instance = new FullTypeNameConverter();

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var c = value as Type;
			if (c == null) return value;
			return c.Name + " (" + c.Namespace + ")";
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}

	public class TypeNameConverter : IValueConverter
	{
		public static TypeNameConverter Instance = new TypeNameConverter();

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value == null) return null;
			var type = value.GetType();
			var typeName = Utils.IsCollection(type) ? "Collection" : type.Name;
			return "(" + type.Name + ")";
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}

	public class EnsureCollectionConverter : IValueConverter
	{
		public static EnsureCollectionConverter Instance = new EnsureCollectionConverter();

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value == null) {
				return null;
			}
			if (value is IEnumerable) {
				return value;
			}
			return new[] { value };
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}

	public class MinConverter : IValueConverter
	{
		public static MinConverter Instance = new MinConverter();

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return Math.Min((int)value, int.Parse(parameter.ToString()));
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}