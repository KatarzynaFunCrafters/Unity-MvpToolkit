﻿using System;
using System.Collections.Generic;
using Behc.Utils;
using JetBrains.Annotations;

namespace Behc.Navigation
{
    public class NavigationRegistry
    {
        private readonly Dictionary<string, INavigable> _navigables = new Dictionary<string, INavigable>();

        [MustUseReturnValue]
        public IDisposable Register(string name, INavigable navigable)
        {
            _navigables.Add(name, navigable);

            return new GenericDisposable(() => _navigables.Remove(name));
        }

        public INavigable Get(string name)
        {
            return _navigables[name];
        }
    }
}