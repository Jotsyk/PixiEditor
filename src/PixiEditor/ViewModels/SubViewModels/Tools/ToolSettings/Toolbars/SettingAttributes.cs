﻿using System.Reflection.Emit;
using PixiEditor.DrawingApi.Core.ColorsImpl;

namespace PixiEditor.ViewModels.SubViewModels.Tools.ToolSettings.Toolbars;

public static class Settings
{
    /// <summary>
    /// A toolbar setting of type <see cref="bool"/>
    /// </summary>
    public class BoolAttribute : SettingsAttribute
    {
        public BoolAttribute(string labelKey) : base(labelKey) { }

        public BoolAttribute(string labelKey, object defaultValue) : base(labelKey, defaultValue) { }
    }

    /// <summary>
    /// A toolbar setting of any enum
    /// </summary>
    public class EnumAttribute : SettingsAttribute
    {
        public EnumAttribute(string labelKey) : base(labelKey) { }

        public EnumAttribute(string labelKey, object defaultValue) : base(labelKey, defaultValue) { }
    }

    /// <summary>
    /// A toolbar setting of type <see cref="Color"/>
    /// </summary>
    public class ColorAttribute : SettingsAttribute
    {
        public ColorAttribute(string labelKey) : base(labelKey) { }

        public ColorAttribute(string labelKey, byte r, byte g, byte b) : base(labelKey, new Color(r, g, b)) { }
        
        public ColorAttribute(string labelKey, byte r, byte g, byte b, byte a) : base(labelKey, new Color(r, g, b, a)) { }
    }

    /// <summary>
    /// A toolbar setting of type <see cref="float"/>
    /// </summary>
    public class FloatAttribute : SettingsAttribute
    {
        public FloatAttribute(string labelKey) : base(labelKey) { }

        public FloatAttribute(string labelKey, float defaultValue) : base(labelKey, defaultValue) { }
    }

    /// <summary>
    /// A toolbar setting of type <see cref="int"/>
    /// </summary>
    public class SizeAttribute : SettingsAttribute
    {
        public SizeAttribute(string labelKey) : base(labelKey) { }
    }

    /// <summary>
    /// Marks a setting to be inherited from the toolbar type
    /// </summary>
    public class InheritedAttribute : SettingsAttribute { }

    [AttributeUsage(AttributeTargets.Property)]
    public abstract class SettingsAttribute : Attribute
    {
        public string Name { get; set; }

        public SettingsAttribute() { }
        
        public SettingsAttribute(string labelKey)
        {
            LabelKey = labelKey;
        }

        public SettingsAttribute(string labelKey, object defaultValue)
        {
            LabelKey = labelKey;
            DefaultValue = defaultValue;
        }
        
        public readonly string LabelKey;

        public readonly object DefaultValue;
    }
}
