﻿using System.Windows;
using System.Windows.Controls;
using ClipboardAnalyzer.DomainModels;

namespace ClipboardAnalyzer.UI
{
    public class CustomDataTemplateSelector : DataTemplateSelector
    {
        public DataTemplate StringTemplate { get; set; }
        public DataTemplate ImageTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is ClipboardData data)
            {
                return StringTemplate;
            }

            if (item is ClipboardImage image)
            {
                return ImageTemplate;
            }

            return StringTemplate;

        }
    }
}
