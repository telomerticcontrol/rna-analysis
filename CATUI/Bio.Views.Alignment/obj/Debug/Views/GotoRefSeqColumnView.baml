   M S B A M L   `   `   `  ����c  _System.Windows.Interactivity, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35qPclr-namespace:System.Windows.Interactivity;assembly=System.Windows.InteractivitySystem.Windows.Interactivity  5   	    Bio.Views.Alignment4   .Bio.Views.Alignment.Views.GotoRefSeqColumnView   5   	   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   [ WJulmar.Wpf.Behaviors, Version=1.0.0.5, Culture=neutral, PublicKeyToken=ff1c121d8ddc1581Y UJulMar.Wpf.Helpers, Version=1.0.0.5, Culture=neutral, PublicKeyToken=ff1c121d8ddc1581/julmar http://www.julmar.com/wpfhelpers   6@   fInteractivityPclr-namespace:System.Windows.Interactivity;assembly=System.Windows.Interactivity   5   	     � Title$"  Jump to Reference Nucelotide��5   	    � WindowStartupLocation$ CenterOwner=�5   	   ���	#FF595959 � ShowInTaskbar .   � 
ResizeMode$ NoResize=�5   	   $	��240��6    $	��500��6-    +� FocusedElement 5   .   ��  �� ElementName$ tb��.��5	      � $��5q�6    � ColumnDefinitions 5
   
   �� 5      $
��Auto �6   �� 5       � RowDefinitions 5   
   �� 5      �� 5      �� 5      .+�5   
   �� $��Reference Sequence:��6   $��Center=�6P   $��5q�6k    �� Target 6.   �� $ rs��6k   �� 5   
   -   5        	 ��Name$	 rs��5      $��1��6   $��Stretch=�60   $	��5,0q�6N   
 �� DisplayMemberPath$
 Header��5   >   $��Center=�5   F   $��5q�6a   ��5      �� *ReferenceSequences+ �� SelectedItem 5      �� *SelectedReferenceSequence+6a   �� 5   
   $��1��6   $"��_Goto Reference Nuceleotide:��6   $��Center=�6f   $��5q�6�    6D   �� $ tb��6�   �� 5   
   -   5         ��Name$ tb��5      $��1��6   $��1��6+   $	��100��6;   $
��Left=�5      $	��5,0q�6-   $��Center=�6:   $��5q�6U   ��5   J   �� *
Position+ �� UpdateSourceTrigger$ PropertyChanged=�.��5   a   .   (System.Windows.Interactivity.Interaction   	Behaviors 5      9  3JulMar.Windows.Interactivity.NumericTextBoxBehavior  5         AllowMouseDrag . 60   .��5   3   5      �� 5!   
    �� Orientation$ 
Horizontal=�6   $��2��6.   $��2��6;   $��Right=�6O   .+�5"      �� $��OK��6    �� 	IsDefault . $
��10,5q�63   $��10q�6B   $	��100��6N   $��Center=�6]    5#      6  0JulMar.Windows.Interactivity.CloseDialogBehavior  5$         DialogResult . 61   5&      �� 5'      $��Cancel��6    �� IsCancel . $
��10,5q�66   $��10q�6E   $	��100��6Q   $��Center=�6`   5(      5)      5*      