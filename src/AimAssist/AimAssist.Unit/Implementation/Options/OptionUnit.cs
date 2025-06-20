﻿using AimAssist.Core;
using AimAssist.Core.Units;

namespace AimAssist.Units.Implementation.Options
{
    public class OptionUnit : IUnit
    {
        public OptionUnit(string name, IEnumerable<string> optionFilePaths)
        {
            Name = name;
            OptionFilePaths = optionFilePaths;
        }

        public IMode Mode => OptionMode.Instance;

        public string Name { get; }

        public string Description => "";

        public string Category => Constants.AppName;

        public IEnumerable<string> OptionFilePaths { get; }
    }
}
