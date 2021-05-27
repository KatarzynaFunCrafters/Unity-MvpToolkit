﻿using System;
using System.Collections.Generic;
using UnityEngine;

namespace Behc.Mvp.Presenter
{
    public class PresenterUpdateKernel : MonoBehaviour
    {
        public bool UpdateLoop { get; private set; }

        private class PresenterItem
        {
            public IPresenter Presenter;
            public PresenterItem Parent;
            public List<PresenterItem> Children;

            public bool RequestedUpdate;
            public bool ChildRequestedUpdate;
        }

        private readonly List<PresenterItem> _rootElements = new List<PresenterItem>();
        private readonly Dictionary<IPresenter, PresenterItem> _elementsMap = new Dictionary<IPresenter, PresenterItem>();

        public void RegisterPresenter(IPresenter presenter, IPresenter parent)
        {
            if (parent == null)
            {
                PresenterItem item = new PresenterItem { Presenter = presenter };
                _rootElements.Add(item);
                _elementsMap.Add(presenter, item);
            }
            else
            {
                PresenterItem parentItem = _elementsMap[parent];
                if (parentItem.Children == null)
                    parentItem.Children = new List<PresenterItem>();

                PresenterItem item = new PresenterItem
                {
                    Presenter = presenter,
                    Parent = parentItem,
                };

                parentItem.Children.Add(item);
                _elementsMap.Add(presenter, item);
            }
        }

        public void UnregisterPresenter(IPresenter presenter)
        {
            PresenterItem item = _elementsMap[presenter];
            if (item.Parent != null)
            {
                item.Parent.Children.Remove(item);
            }
            else
            {
                _rootElements.Remove(item);
            }

            _elementsMap.Remove(presenter);

            if (item.Children != null && item.Children.Count > 0)
                throw new Exception("unregister non empty presenter!");
        }

        public void RequestUpdate(IPresenter presenter)
        {
            PresenterItem item = _elementsMap[presenter];
            item.RequestedUpdate = true;

            PresenterItem parent = item.Parent;
            while (parent != null)
            {
                parent.ChildRequestedUpdate = true;
                parent = parent.Parent;
            }
        }

        public void InitializePresenters(Action<PresenterUpdateKernel> initCallback)
        {
            UpdateLoop = true;
            initCallback?.Invoke(this);
            UpdateLoop = false;
        }

        private void Update()
        {
            UpdateLoop = true;

            foreach (PresenterItem item in _rootElements)
            {
                UpdateItem(item);
            }

            UpdateLoop = false;
        }

        private void UpdateItem(PresenterItem item)
        {
            if (item.RequestedUpdate)
                item.Presenter.ScheduledUpdate();

            if (item.ChildRequestedUpdate && item.Children != null)
            {
                foreach (PresenterItem child in item.Children)
                {
                    UpdateItem(child);
                }
            }

            item.RequestedUpdate = false;
            item.ChildRequestedUpdate = false;
        }
    }
    
    public static class TestCounter
    {
        public static int Counter = 0;
    }
}