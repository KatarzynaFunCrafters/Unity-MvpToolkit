﻿using Behc.Mvp.Presenter;
using Behc.Mvp.Utils;

namespace Behc.Mvp.DataStack
{
    public class DataStackPresenter :  AnimatedPresenterBase<DataStack>
    {
        public override void Bind(object model, IPresenter parent, bool prepareForAnimation)
        {
            base.Bind(model, parent, prepareForAnimation);
    
            if (_curtain != null)
            {
                DisposeOnUnbind(_curtain.OnTrigger.Subscribe(CurtainClicked));

                if (prepareForAnimation && _items.Count > 0)
                {
                    _curtain.Show(_items.Count * _sortingStep);
                }
                else
                {
                    _curtain.Setup(_items.Count > 0, _items.Count * _sortingStep);
                }
            }
        }

        public override void Activate()
        {
            base.Activate();

            if (_items.Count > 0)
            {
                ItemDesc topLevel = _items[_items.Count - 1];
                if (topLevel.State == ItemState.READY)
                {
                    topLevel.Presenter.Activate();
                    topLevel.Active = true;
                }
            }
        }
        
        public override void Deactivate()
        {
            if (_items.Count > 0)
            {
                ItemDesc topLevel = _items[_items.Count - 1];
                if (topLevel.Active)
                {
                    topLevel.Presenter.Deactivate();
                    topLevel.Active = false;
                }
            }

            base.Deactivate();
        }

        protected override void UpdateContent()
        {
            int oldCount = _items.Count;
            base.UpdateContent();
 
            for (int index = 0; index < _items.Count - 1; index++)
            {
                ItemDesc desc = _items[index];
                if (desc.Active)
                {
                    desc.Presenter.Deactivate();
                    desc.Active = false;
                }
            }

            if (_items.Count > 0 && IsActive)
            {
                ItemDesc topLevel = _items[_items.Count - 1];
                if (!topLevel.Active && topLevel.State == ItemState.READY)
                {
                    topLevel.Presenter.Activate();
                    topLevel.Active = true;
                }
            }

            if (_curtain != null)
            {
                if (_items.Count == 0)
                {
                    if (oldCount > 0)
                        _curtain.Hide();
                }
                else
                {
                    if (oldCount == 0)
                        _curtain.Show(_items.Count * _sortingStep);
                    else if (oldCount != _items.Count)
                        _curtain.Switch(_items.Count * _sortingStep, oldCount * _sortingStep);
                }
            }
        }

        private void CurtainClicked()
        {
            if (_items.Count == 0)
                return;

            ItemDesc topLevel = _items[_items.Count - 1];
            if (topLevel.Presenter is IPresenterStackOptions { CanDefaultClose: false })
                return;

            _model.Remove(topLevel.Model);
        }
    }
}