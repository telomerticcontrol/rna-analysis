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

using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Threading;
using Bio.Data.Providers.Interfaces;
using System;
using System.Windows.Input;
using JulMar.Windows.Mvvm;
using JulMar.Windows.Interfaces;
using System.Runtime.CompilerServices;

// Primary BioBrowser assembly can invoke internal methods
[assembly: InternalsVisibleToAttribute("BioBrowser")] 

namespace Bio.Views.ViewModels
{
    /// <summary>
    /// This represents the base class for all BioViewModels
    /// </summary>
    [DebuggerDisplay("{Title}")]
    public abstract class BioViewModel : ViewModel
    {
        private string _title;
        private bool _isDocked;

        /// <summary>
        /// Window associated with this view model
        /// </summary>
        public Window CurrentWindow { get; set; }

        /// <summary>
        /// Visual associated with this view model
        /// </summary>
        public FrameworkElement Visual { get; set; }

        /// <summary>
        /// Title for the view
        /// </summary>
        public virtual string Title
        {
            get { return _title; }
            set { _title = value; OnPropertyChanged("Title"); }
        }

        /// <summary>
        /// The edit menu for this view
        /// </summary>
        public ObservableCollection<MenuItem> EditMenu { get; private set; }

        /// <summary>
        /// The view menu for this view
        /// </summary>
        public ObservableCollection<MenuItem> ViewMenu { get; private set; }

        /// <summary>
        /// The options menu for this view
        /// </summary>
        public ObservableCollection<MenuItem> OptionsMenu { get; private set; }

        /// <summary>
        /// This method is used to initialize the view
        /// </summary>
        /// <param name="data">BioData loader interface</param>
        /// <returns>True = success, false = failed</returns>
        public abstract bool Initialize(IBioDataLoader data);

        /// <summary>
        /// This method is used to change floating windows to docked windows and vice-versa.
        /// </summary>
        protected abstract void OnDockStyleChanged();

        /// <summary>
        /// Invoked to close the view
        /// </summary>
        public ICommand CloseCommand { get; private set; }

        /// <summary>
        /// The PackUri to the icon to display for this visual
        /// </summary>
        public string ImageUrl { get; set; }

        /// <summary>
        /// Returns the current dock state of the view
        /// </summary>
        public bool IsDocked
        {
            get { return _isDocked; }
            set 
            {
                _isDocked = value; 
                OnPropertyChanged("IsDocked");
                OnDockStyleChanged();
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        protected BioViewModel()
        {
            _isDocked = true;
            OptionsMenu = new ObservableCollection<MenuItem>();
            EditMenu = new ObservableCollection<MenuItem>();
            ViewMenu = new ObservableCollection<MenuItem>();
            CloseCommand = new DelegatingCommand(RaiseCloseRequest);
        }

        /// <summary>
        /// This can be overridden to provide initialization when the view is created.
        /// </summary>
        protected virtual void OnCreatedView()
        {
        }

        /// <summary>
        /// Called when this becomes the "active" view in the tab list.
        /// </summary>
        protected internal virtual void OnActivate()
        {
        }

        /// <summary>
        /// Called when this loses the "active" view status
        /// </summary>
        protected internal virtual void OnDeactivate()
        {
        }
    }

    /// <summary>
    /// A UI creator version that manages the connection between the View
    /// and ViewModel and creates the View on the Initialize call.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class BioViewModel<T> : BioViewModel
    {
        /// <summary>
        /// This initializes the view model and generates the appropriate view.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public override bool Initialize(IBioDataLoader data)
        {
            // Create the visual
            Visual = (FrameworkElement) Activator.CreateInstance(typeof (T));
            Visual.DataContext = this;

            if (IsDocked == true)
                SendMessage(ViewMessages.AddDockedView, this);
            else
            {
                CurrentWindow = new BioFloatingWindow { DataContext = this };
                CurrentWindow.Closed += win_Closed;
                CurrentWindow.Show();
                CurrentWindow.Activate();
            }

            OnCreatedView();
            SendMessage(ViewMessages.ViewCreated, this);
            return true;
        }

        /// <summary>
        /// This event is called when a floating window is closed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void win_Closed(object sender, EventArgs e)
        {
            Window win = (Window) sender;
            win.Closed -= win_Closed;
            SendMessage(ViewMessages.ViewClosed, this);
        }

        /// <summary>
        /// This method is used to change floating windows to docked windows and vice-versa.
        /// </summary>
        protected override void OnDockStyleChanged()
        {
            IDisposable waitUI = null;
            var notificationVisualizer = Resolve<INotificationVisualizer>();
            if (notificationVisualizer != null)
                waitUI = notificationVisualizer.BeginWait(string.Empty, string.Empty);

            using (waitUI)
            {
                if (CurrentWindow != null && IsDocked == true)
                {
                    CurrentWindow.Visibility = Visibility.Hidden;
                    CurrentWindow.Content = null;
                    CurrentWindow.Closed -= win_Closed;
                    CurrentWindow.Close();
                    CurrentWindow = null;

                    SendMessage(ViewMessages.AddDockedView, this);
                }
                else
                {
                    SendMessage(ViewMessages.RemoveDockedView, this);
                    CurrentWindow = new BioFloatingWindow {DataContext = this};
                    CurrentWindow.Closed += win_Closed;
                    CurrentWindow.Show();
                    
                    Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background, (Action) (() => CurrentWindow.Activate()));
                }
            }
        }

        /// <summary>
        /// This raises the ActivateRequest event to activate the UI.
        /// </summary>
        public override void RaiseActivateRequest()
        {
            if (!IsDocked)
            {
                CurrentWindow.Activate();
            }

            base.RaiseActivateRequest();
        }

        /// <summary>
        /// This raises the CloseRequest event to close the UI.
        /// </summary>
        public override void RaiseCloseRequest()
        {
            if (IsDocked)
            {
                SendMessage(ViewMessages.RemoveDockedView, this);
                SendMessage(ViewMessages.ViewClosed, this);
            }
            else
            {
                CurrentWindow.Close();
            }

            base.RaiseCloseRequest();
        }
    }
}