/* 
* Copyright (c) 2009, The University of Texas at Austin
* All rights reserved.
*
* Redistribution and use in source and binary forms, with or without modification, 
* are permitted provided that the following conditions are met:
*
* 1. Redistributions of source code must retain the above copyright notice, 
* this list of conditions and the following disclaimer.
*
* 2. Redistributions in binary form must reproduce the above copyright notice, 
* this list of conditions and the following disclaimer in the documentation and/or other materials 
* provided with the distribution.
*
* Neither the name of The University of Texas at Austin nor the names of its contributors may be 
* used to endorse or promote products derived from this software without specific prior written 
* permission.
* 
* THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR 
* IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND 
* FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS 
* BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES 
* (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR 
* PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN 
* CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF 
* THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using Bio.Views.Interfaces;
using JulMar.Windows.Mvvm;

namespace Bio.Views.ViewModels
{
    /// <summary>
    /// This ViewModel is used for all SideBar items that can be docked to the sidebar, or left floating.
    /// </summary>
    public class SidebarViewModel<T> : ViewModel, ISidebarViewItem
                                        where T : FrameworkElement
    {
        private string _title;
        private bool _changingDockState;
        private FrameworkElement _visual;

        /// <summary>
        /// The static view data we track - whether the window is floating/docked and 
        /// the floating window if it's visible.
        /// </summary>
        internal class ViewData
        {
            public BioFloatingWindow Window { get; set;}
            public bool IsDocked { get; set; }
        }

        private static readonly Dictionary<Type, ViewData> _viewData = new Dictionary<Type, ViewData>();

        /// <summary>
        /// Raised when the view is closed.
        /// </summary>
        public event Action<ISidebarViewItem> ViewClosed;

        /// <summary>
        /// Displayed on the titlebar or header of the expander
        /// </summary>
        public string Title
        {
            get { return _title; }
            set 
            { 
                _title = value;
                OnPropertyChanged("Title"); 
            }
        }

        /// <summary>
        /// The PackUri to the icon to display for this visual
        /// </summary>
        public string ImageUrl { get; set; }

        /// <summary>
        /// The visual to display
        /// </summary>
        public object Visual
        {
            get
            {
                if (_visual == null)
                {
                    _visual = Activator.CreateInstance<T>();
                    _visual.DataContext = this;
                }
                return _visual;
            }
        }

        /// <summary>
        /// Whether this is floating or docked in the main UI
        /// </summary>
        public bool IsDocked
        {
            get { return GetViewData().IsDocked; }
            set 
            {
                GetViewData().IsDocked = value;
                OnPropertyChanged("IsDocked");
                ChangeDockState();
            }
        }

        /// <summary>
        /// Called to close the view
        /// </summary>
        public ICommand CloseView { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public SidebarViewModel()
        {
            CloseView = new DelegatingCommand(OnCloseView);
        }

        /// <summary>
        /// Returns the view data for this sidebar item
        /// </summary>
        /// <returns></returns>
        private ViewData GetViewData()
        {
            lock(_viewData)
            {
                ViewData viewData;
                if (!_viewData.TryGetValue(GetType(), out viewData))
                {
                    viewData = new ViewData();
                    _viewData[GetType()] = viewData;
                }
                return viewData;
            }
        }

        /// <summary>
        /// Called to close the floating or docked view
        /// </summary>
        private void OnCloseView()
        {
            ViewData vd = GetViewData();

            if (vd.Window != null)
            {
                vd.Window.Content = null;
                vd.Window.Close();
                vd.Window = null;
            }

            SendMessage<ISidebarViewItem>(ViewMessages.RemoveFloatingWindowFromSidebar, this);
            OnViewClosed();
            
            _visual = null;
        }

        /// <summary>
        /// This displays the sidebar view window
        /// </summary>
        public void Show()
        {
            // First one created?
            if (IsDocked || GetViewData().Window == null)
            {
                // Force the window creation
                ChangeDockState();
            }
            else
            {
                RaiseActivateRequest();
            }
        }

        /// <summary>
        /// Turns toplevel floating on and off.
        /// </summary>
        public void SetActiveViewModel(BioViewModel activeVm)
        {
            ViewData vd = GetViewData();
            if (vd.Window != null)
            {
                bool isFloating = (!activeVm.IsDocked && !vd.IsDocked);
                vd.Window.Owner = isFloating ? activeVm.CurrentWindow : Application.Current.MainWindow;
            }
        }

        /// <summary>
        /// Changes the docked state to/from floating
        /// </summary>
        private void ChangeDockState()
        {
            _changingDockState = true;

            try
            {
                ViewData vd = GetViewData();

                if (vd.IsDocked)
                {
                    if (vd.Window != null)
                    {
                        vd.Window.Content = null;
                        vd.Window.Close();
                        vd.Window = null;
                    }
                    SendMessage<ISidebarViewItem>(ViewMessages.AddFloatingWindowToSidebar, this);
                }
                else // floating
                {
                    SendMessage<ISidebarViewItem>(ViewMessages.RemoveFloatingWindowFromSidebar, this);
                    vd.Window = new BioFloatingWindow
                        {
                            DataContext = this,
                            NotifyViewActivate = false,
                            Width = 400, Height = 400,
                        };

                    vd.Window.Closing += OnWindowClosed;
                    vd.Window.Show();
                    vd.Window.Activate();
                }
            }
            finally
            {
                _changingDockState = false;
            }
        }

        /// <summary>
        /// Called when the window is closed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnWindowClosed(object sender, EventArgs e)
        {
            ViewData vd = GetViewData();
            Debug.Assert(vd.Window == sender);
            vd.Window.Content = null;
            vd.Window.Closing -= OnWindowClosed;
            vd.Window = null;

            if (!_changingDockState)
                OnViewClosed();
        }

        /// <summary>
        /// Hook for derived types to detect view closed.
        /// </summary>
        protected virtual void OnViewClosed()
        {
            if (ViewClosed != null)
                ViewClosed(this);
        }

        /// <summary>
        /// Called to activate the floating window.
        /// </summary>
        public override void RaiseActivateRequest()
        {
            ViewData vd = GetViewData();
            if (vd.Window != null)
            {
                vd.Window.Activate();
            }
            else if (!vd.IsDocked)
            {
                ChangeDockState();
            }
        }
    }
}
