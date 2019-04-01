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

using System.Diagnostics;
using System.Windows.Controls;
using JulMar.Windows.Extensions;
using JulMar.Windows.Mvvm;
using Bio.Views.Alignment.ViewModels;
using Bio.Views.Alignment.Internal;

namespace Bio.Views.Alignment.Views
{
    /// <summary>
    /// Interaction logic for TaxonmyJumpView.xaml
    /// </summary>
    public partial class TaxonmyJumpView
    {
        public TaxonmyJumpView()
        {
            InitializeComponent();

            // Register with the message mediator to be notifed when the view changes
            var mediator = ViewModel.ServiceProvider.Resolve<MessageMediator>();
            if (mediator != null)
                mediator.RegisterHandler<AlignmentViewModel>(AlignmentViewMessages.SwitchAlignmentView, OnViewChanged);
        }

        /// <summary>
        /// This is invoked when the view changes and our taxonomy view needs to be updated.
        /// </summary>
        /// <param name="vm"></param>
        private void OnViewChanged(AlignmentViewModel vm)
        {
            DataContext = vm.TaxonomyViewModel;
        }

        private void tv_SelectedItemChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<object> e)
        {
            TaxonomyJumpViewModel vm = DataContext as TaxonomyJumpViewModel;
            if (vm != null)
                vm.SelectionChanged.Execute(e.NewValue);
        }
    }
}
