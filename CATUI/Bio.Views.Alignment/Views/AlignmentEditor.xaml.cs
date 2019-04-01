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

using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using Bio.Views.Alignment.ViewModels;
using JulMar.Windows;
using Bio.Views.Alignment.Internal;
using Bio.Views.Alignment.Options;
using System.Windows.Controls;

namespace Bio.Views.Alignment.Views
{
    /// <summary>
    /// View for the alignment editor
    /// </summary>
    public partial class AlignmentEditor
    {
        private PropertyObserver<RuntimeOptionsViewModel> _optionObserver;

        /// <summary>
        /// Constructor
        /// </summary>
        public AlignmentEditor()
        {
            Loaded += OnEditorLoaded;
            DataContextChanged += OnDataChanged;

            // Catch navigation gestures (flicks from tablet/touch).
            CommandBindings.Add(new CommandBinding(NavigationCommands.BrowseBack, OnNavigationCommandExecuted));
            CommandBindings.Add(new CommandBinding(NavigationCommands.BrowseForward, OnNavigationCommandExecuted));
            CommandBindings.Add(new CommandBinding(ComponentCommands.ScrollPageUp, OnNavigationCommandExecuted));
            CommandBindings.Add(new CommandBinding(ComponentCommands.ScrollPageDown, OnNavigationCommandExecuted));
            CommandBindings.Add(new CommandBinding(ScrollBar.PageRightCommand, OnNavigationCommandExecuted));
            CommandBindings.Add(new CommandBinding(ScrollBar.PageLeftCommand, OnNavigationCommandExecuted));
            CommandBindings.Add(new CommandBinding(ScrollBar.ScrollToLeftEndCommand, OnNavigationCommandExecuted));
            CommandBindings.Add(new CommandBinding(ScrollBar.ScrollToRightEndCommand, OnNavigationCommandExecuted));

            InitializeComponent();
        }

        /// <summary>
        /// Sets up the initial view
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnEditorLoaded(object sender, RoutedEventArgs e)
        {
            LeftHeader.Columns.CollectionChanged += Columns_CollectionChanged;
            LeftAlignmentView.SizeChanged += OnLeftAlignmentSizeChanged;
            
            OnShowHideRowNumbers();
        }

        /// <summary>
        /// This repositions the horizontal scroll bar when the column collection is reordered
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Columns_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Move)
                CheckRepositionHorizontalScrollbar();
        }

        /// <summary>
        /// DataContext has been set or changed on the visual.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnDataChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            _optionObserver = null;
            if (ViewModel == null)
                return;

            _optionObserver = new PropertyObserver<RuntimeOptionsViewModel>(ViewModel.Options);
            _optionObserver.RegisterHandler(fe => fe.ShowRowNumbers, od => OnShowHideRowNumbers());
        }

        /// <summary>
        /// Retrieves the view model
        /// </summary>
        private AlignmentViewModel ViewModel
        {
            get { return (AlignmentViewModel)DataContext; }
        }

        /// <summary>
        /// Called when a column changes size
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnLeftAlignmentSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (LeftHeader != null)
            {
                CheckRepositionHorizontalScrollbar();
            }
        }

        /// <summary>
        /// Called to show/hide the row numbers
        /// </summary>
        private void OnShowHideRowNumbers()
        {
            GridViewColumn rowNumColumn = (GridViewColumn)FindResource("RowNumberColumnHeader");
            if (ViewModel.Options.ShowRowNumbers)
            {
                if (!LeftHeader.Columns.Contains(rowNumColumn))
                    LeftHeader.Columns.Insert(0, rowNumColumn);
            }
            else
            {
                LeftHeader.Columns.Remove(rowNumColumn);
            }

            CheckRepositionHorizontalScrollbar();
        }

        /// <summary>
        /// This repositions the horizontal scrollbar
        /// </summary>
        private void CheckRepositionHorizontalScrollbar()
        {
            double specifiedWidth = LeftHeader.Columns.Where(hc => !(hc.Header is HeaderRulerIndex)).Sum(hc => hc.ActualWidth);
            if (specifiedWidth == 0)
                return;

            var column = LeftHeader.Columns.First(hc => hc.Header is HeaderRulerIndex);

            double newWidth = (LeftAlignmentView.ActualWidth - specifiedWidth);
            if (!ViewModel.IsScreenSplit)
                newWidth -= SystemParameters.ScrollWidth;

            if (newWidth >= 0 && newWidth != column.Width)
                hsbLeft.Width = column.Width = newWidth;

            double leadingWidth = Enumerable.Range(0, LeftHeader.Columns.IndexOf(column)).Sum(i => LeftHeader.Columns[i].ActualWidth);
            hsbLeft.Margin = new Thickness(leadingWidth, 0, 0, 0);
            if (ViewModel.IsScreenSplit)
            {
                RightHeader.Columns[0].Width = hsbRight.Width = RightAlignmentView.ActualWidth - SystemParameters.ScrollWidth;
            }
        }

        /// <summary>
        /// This triggers commands on PreviewKeyDown so we can override scrolling behavior
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            e.Handled = (HandleKeystroke(e.Key, Keyboard.Modifiers));
            base.OnPreviewKeyDown(e);
        }

        /// <summary>
        /// This processes a given keystroke
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="modifiers">Modifier</param>
        /// <returns></returns>
        private bool HandleKeystroke(Key key, ModifierKeys modifiers)
        {
            foreach (var inputBinding in InputBindings)
            {
                KeyBinding keyBinding = inputBinding as KeyBinding;
                if (keyBinding == null)
                    continue;

                if (keyBinding.Key == key && keyBinding.Modifiers == modifiers)
                {
                    // Invoke the command
                    ICommand command = keyBinding.Command;
                    if (command != null)
                    {
                        var commandTarget = keyBinding.CommandTarget ?? this;

                        // Normal (MVVM) command
                        if (commandTarget == null || !typeof(RoutedCommand).IsAssignableFrom(command.GetType()))
                        {
                            if (command.CanExecute(keyBinding.CommandParameter))
                                command.Execute(keyBinding.CommandParameter);
                        }
                        // Target a specific UI element via RoutedCommand
                        else
                        {
                            RoutedCommand rc = (RoutedCommand)command;
                            if (rc.CanExecute(keyBinding.CommandParameter, commandTarget))
                                rc.Execute(keyBinding.CommandParameter, commandTarget);
                        }
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Horizontal scroll handling
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnHorizontalScroll(object sender, ScrollEventArgs e)
        {
            if (e.ScrollEventType != ScrollEventType.ThumbTrack)
            {
                ((SplitPaneViewModel) ((ScrollBar) sender).DataContext).FirstColumn = (int) e.NewValue;
            }
        }

        /// <summary>
        /// This method is called when a navigation event is detected.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnNavigationCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Command == NavigationCommands.BrowseBack)
            {
                HandleKeystroke(Key.PageUp, ModifierKeys.Control);
            }
            else if (e.Command == NavigationCommands.BrowseForward)
            {
                HandleKeystroke(Key.PageDown, ModifierKeys.Control);
            }
            else if (e.Command == ComponentCommands.ScrollPageUp)
            {
                ScrollViewer sv = (ScrollViewer)LeftAlignmentView.Template.FindName("PART_ScrollViewer", LeftAlignmentView);
                if (sv != null)
                {
                    ScrollBar sb = (ScrollBar)sv.Template.FindName("PART_VerticalScrollBar", sv);
                    if (sb != null)
                    {
                        ScrollBar.PageUpCommand.Execute(e.Parameter, sb);
                    }
                }
            }
            else if (e.Command == ComponentCommands.ScrollPageDown)
            {
                ScrollViewer sv = (ScrollViewer)LeftAlignmentView.Template.FindName("PART_ScrollViewer", LeftAlignmentView);
                if (sv != null)
                {
                    ScrollBar sb = (ScrollBar)sv.Template.FindName("PART_VerticalScrollBar", sv);
                    if (sb != null)
                    {
                        ScrollBar.PageDownCommand.Execute(e.Parameter, sb);
                    }
                }
            }
            else if (e.Command == ScrollBar.PageLeftCommand)
            {
                ScrollBar.PageLeftCommand.Execute(e.Parameter, 
                    (ViewModel.ActiveView == ViewModel.LeftView) ? hsbLeft : hsbRight);
            }
            else if (e.Command == ScrollBar.PageRightCommand)
            {
                ScrollBar.PageRightCommand.Execute(e.Parameter,
                    (ViewModel.ActiveView == ViewModel.LeftView) ? hsbLeft : hsbRight);
            }
            else if (e.Command == ScrollBar.ScrollToLeftEndCommand)
            {
                ScrollBar.ScrollToLeftEndCommand.Execute(e.Parameter,
                    (ViewModel.ActiveView == ViewModel.LeftView) ? hsbLeft : hsbRight);
            }
            else if (e.Command == ScrollBar.ScrollToRightEndCommand)
            {
                ScrollBar.ScrollToRightEndCommand.Execute(e.Parameter,
                    (ViewModel.ActiveView == ViewModel.LeftView) ? hsbLeft : hsbRight);

            }
        }
    }
}