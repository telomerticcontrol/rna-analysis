   M S B A M L   `   `   `  ����N  JBio.Views.Alignment, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullK*clr-namespace:Bio.Views.Alignment.ControlsBio.Views.Alignment.Controls  5   L   Y UJulMar.Wpf.Helpers, Version=1.0.0.5, Culture=neutral, PublicKeyToken=ff1c121d8ddc1581aCclr-namespace:JulMar.Windows.Converters;assembly=JulMar.Wpf.HelpersJulMar.Windows.Converters 6�   K*clr-namespace:Bio.Views.Alignment.InternalBio.Views.Alignment.Internal  6�   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e0890    *Bio.Views.Alignment.Controls.SequenceBlock  �� 
TargetType �� Property �� Value �� 
TargetType � UseLayoutRounding@   :Bio.Views.Alignment.Controls.SequenceTextDrawingVisualHost  Name ��Name    FocusBackgroundBrush    FocusBorderBrush	 �� RelativeSource
 �� Mode �� 	Converter-  'JulMar.Windows.Converters.MathConverter �� ConverterParameter T� Y T� X ��Name    FocusedSymbolBrush �� Triggers ]� 
Conditions �� Property �� Value �� 
TargetName�� 5      D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    :x,http://schemas.microsoft.com/winfx/2006/xaml     5      7Controls*clr-namespace:Bio.Views.Alignment.Controls  6L   NjulmarCclr-namespace:JulMar.Windows.Converters;assembly=JulMar.Wpf.Helpers  6�   7Internal*clr-namespace:Bio.Views.Alignment.Internal  6�   %f  '         �� 5      "    6   .�5   
   ��  � �� 6(   Z� �� 5   
    � �� 5      �� 5      "   6"   .R�5	      �  . .+�5
      �� #��z��6!   $��1q�6M   #��z��6a   $��Center=�6�   ._�5        $ PART_TextHost��6E   $��Center=�6\   5      �� 5      $ bd��6!   $��1q�6-   $y�	Collapsed=�6A   #��z 5   !   #��z 5   !   $
��Left=�6R   $��Center=�6m   ��5   !   �� *	CellWidth+	 �� $
 TemplatedParent=�   $ +2����5   !   �� *
CellHeight+	 �� $
 TemplatedParent=�   $ +2��z�5      T� 5   "   $ 0Q�65    6;   �� *FocusRectangleOffset+	 �� $
 TemplatedParent=�._�5      �� $ tb��6(   $
��Bold"�64   #��z 5   (   $��Center=�6Z   $��Center=�6w   ��5   F   �� *FocusedSymbol.Text+	 �� $
 TemplatedParent=�5   w   5      5       5      ]� 5       5      �� 5   "    �@  ShowFocusIndicator . 6J   �� 5   "    �@  IsActive . 6@   ./�5      �� $ bd��6%    � y�$ Visible=�6K   5       5"      5$      5%      5'      