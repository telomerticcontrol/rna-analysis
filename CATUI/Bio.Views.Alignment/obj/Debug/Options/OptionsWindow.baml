   M S B A M L   `   `   `  ����c  _System.Windows.Interactivity, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35qPclr-namespace:System.Windows.Interactivity;assembly=System.Windows.InteractivitySystem.Windows.Interactivity  5   	   S OBio.Controls.ColorPicker, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullM<clr-namespace:Bio.Controls;assembly=Bio.Controls.ColorPickerBio.Controls 5   	    Bio.Views.Alignment/   )Bio.Views.Alignment.Options.OptionsWindow   5   	   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   ^iPclr-namespace:System.Windows.Interactivity;assembly=System.Windows.Interactivity     5   	   [ WJulmar.Wpf.Behaviors, Version=1.0.0.5, Culture=neutral, PublicKeyToken=ff1c121d8ddc1581Y UJulMar.Wpf.Helpers, Version=1.0.0.5, Culture=neutral, PublicKeyToken=ff1c121d8ddc1581/julmar http://www.julmar.com/wpfhelpers   5   	   MControls<clr-namespace:Bio.Controls;assembly=Bio.Controls.ColorPicker   5   	     � WindowStartupLocation$  CenterOwner=�5   	    � WindowStyle$ SingleBorderWindow=�6-    � 
ResizeMode$ 	CanResize=�6N    � Title$ Options Window��5   	   $	��600��6    $	��800��6-   
�������.��5	      � -   5         �Name$ 
LayoutRoot��5	       � 	Resources 5   
   6  0JulMar.Windows.Converters.KeyToResourceConverter   KeyToResourceConverter   Element �� ElementName  optionTreeList �� ItemsSource	 ��Name
 � ToolTip �� Triggers �� Value �� Binding �� 
TargetName �� Property �� Value  FontsAndColors � RowDefinitions � ColumnDefinitions �� SelectedItem ��Name �� Orientation  Bio.Controls.ColorPicker   Color �� Path �� Mode1  +System.Diagnostics.PresentationTraceSources   
TraceLevel d� 	IsChecked  ReferenceSeqColors ��Name �� SelectedIndex  NucleotideColors �� 	MaxLength  Grouping �� UpdateSourceTrigger.   (System.Windows.Interactivity.Interaction   	Behaviors9  3JulMar.Windows.Interactivity.NumericTextBoxBehavior    AllowMouseDrag��%�  &	        &	 -     &	 ;    &	 v	    &	 }    &	 �      5       6K   �� $ 
LayoutRoot���� 5       6>   �� *
Children+.R�5      �� $	 tb��6   ��6(   �� *Name+
 6>   �� *Description+ 5      �� 5      $
 True��6A    6"   �� *
IsSelected+.a�5      �� $ tb��6!    � ��$
 Bold"�6G   5      5      �� 5      .R�5      �  5      �� 5      $
��Auto �6(   �� 5      $
��Auto �6(   �� 5      $
��Auto �6(   �� 5      $
��Auto �6(   �� 5      �� 5      $
��Auto �6(    5"      �� 5#      �� 5$      $��2* �6+   .+�5'      �� $��Center=�6   $��Header Font:��67   �� 5(      $��1��6   $��4q�6/   $��Center=�6:   ��6U   �� *	FontNames+ 6w   �� *SelectedFont+��5)      �� 5*      .R�5+   "   $� 5,      ��5.      �� 5/      .R�50   "   �� ��6,   �� ��6=   �� 51      53      �� 55      $��1��6   $��Center=�6*   $��Header Font Size:��6E   �� 56      $��1��6   $��1��6,   $��4q�6<   $��Center=�6G   ��57      �� *	FontSizes+ 6A   �� *SelectedFontSize+�� 59      $��2��6   $��Center=�6)   $��Alignment Font:��6D   �� 5:      $��2��6   $��1��6,   $��4q�6<   $��Center=�6G   ��6b   �� *FixedFontNames+ 6�   �� *AlignmentFontFamily+��5;      �� 5<      .R�5=   "   $� 5>      ��5@      �� 5A      .R�5B   "   �� ��6,   �� ��6=   �� 5C      5E      �� 5G      $��3��6   $��Center=�6*   $��Alignment Font Size:��6E   �� 5H      $��3��6   $��1��6,   $��4q�6<   $��Center=�6G   ��5I      �� *	FontSizes+ 6A   �� *AlignmentFontSize+�� 5K      $��4��6   $��Colors��6,   $��4q�6<   $��2��6G   .��5L      �  5M      �� 5N   "   $	��.5* �63   �� 5O   "   .+�5R      �� $ colorOptions��6&   ��6<   �� *ColorOptions+��5S   "   �� 5T   &   .R�5U   *   �� $ 
Horizontal=�65   .+�5V   .   �� 
���   �$��1��6^   $��10��6r   $��10��6}   ��68   �� *Value+6}   �� 5W   .   $	��5,0q�68   ��6E   �� *Name+5X   +   5Y   '   5[        5]      $��1��63    5^   !   �� $ colorOptions��$ SelectedItem.Value�$ TwoWay=�$
 High=�5`      5a      �� 5c      $��2��6   $��5��63   $��4q�6@   $��Center=�6K   $��Show Row Numbers��6f    6�   �� *ShowRowNumbers+5e      5f      �� 5h      .R�5i      �  5j      �� 5k      �� 5l      $
��Auto �6(   .+�5o      �� $ 	refColors��6   $��5q�6a   ��61   �� *ReferenceSequenceColors+��5p      �� 5q      .R�5r   "   �� 
���   �$��1��6L   $��25��6`   $	��300��6l   ��6,   �� 6l   5s      5u      � 5w      $��1��6   $��Center=�6(   $��Stretch=�6C    5x      �� 5y      $
��Auto �6/   �� 5z      .+�5|      �� $��Remove _Color��6!   $
��10,5q�69   $��5q�6H   ��6S   �� *RemoveRefSeqColor+��6y   �� $ 	refColors��$ SelectedItem�� 5}      $��1��6   $��5q�6/    5~      �� 5   "   �� 5�   "   $��15 �63   .+�5�      �� $ refAllColors��6'   $ 0��6_   $��Disabled=�5�   '   $��2��6]   $��Stretch=�6q   ��5�   =   �� *	AllColors+��5�   "   �� 5�   &   .R�5�   *   � $	��300��64   5�   '   ��5�   "   �� 5�   &   .R�5�   *   �� 
���   �$��1��6C   $��10��6h   $��10��6s   $��Center=�6�   $��Center=�6�   ��6W   �� 
 6   �� 6�   5�   '   5�      �� 5�      $
��10,5q�6%   ��64   �� *AddRefSeqColor+��6W   �� $ refAllColors��$ SelectedItem�.��5�   "   �� $ 
Horizontal=�6-   .+�5�   &   �� 
���   �$��1��6?   $��50��6�   $��10��6�   $��Center=�6�   $��Center=�6�   ��6S   �� $ refAllColors��$ SelectedItem�
 6�   �� 6�   �� 5�   &   $��5q�6,   ��67   �� $ refAllColors��$ SelectedItem�5�   #   5�      5�      5�      5�      5�      �� 5�      .R�5�      �  5�      �� 5�      $
��Auto �6(   �� 5�       5�      �� 5�      $
��Auto �6+   �� 5�      .+�5�      �� $ nucList��6   $��5q�6/   $��2��6:   $ 0��6t   ��6K   �� *NucleotideColors+��5�      �� 5�      .R�5�   "   �� $ 
Horizontal=�6-   $��2q�6F   .+�5�   &   �� 
���   �$��1��6?   $��0,0,5,0q�6j   $��20��6{   $��10��6�   ��6S   �� *Color+6�   �� 5�   &   $��75��6H   ��60   �� *Symbol+6H   5�   #   5�      5�      � 5�      $��1��6   $��5q�6+    5�      �� 5�      $
��Auto �6/   �� 5�       5�      �� 5�      $
��Auto �6,   �� 5�      $
��Auto �6,   .+�5�      �� $��Symbol:��6    $��5q�62   $��Center=�6=   �� 5�      $��1��6"   $��Stretch=�6q   $��Center=�6�   $ 1��6�   ��62   �� $ nucList��$ SelectedItem.Symbol�6�   �� 5�      $��1��6    $��Color:��6-   $��5q�6>   $��Center=�6I   �� 5�      $��1��6#   $��1��60   $��Disabled=�5�   '   $��2��6]   $��Stretch=�6q   ��5�   @   �� *	AllColors+ 6b   �� $ nucList��$ SelectedItem.Color���5�      �� 5�   "   .R�5�   &   � $	��300��60   5�   #   ��5�      �� 5�   "   .R�5�   &   �� 
���   �$��1��6?   $��10��6d   $��10��6o   $��Center=�6�   $��Center=�6�   ��6S   �� 
 6{   �� 6�   5�   #   5�      5�      �� 5�      $��2��6!   $��1��6.   $ 
Horizontal=�6>   $��Right=�6W   $��Bottom=�6s   .+�5�      �� $��_Add Symbol��6!   $
��10,5q�6^   $��5 q�6m   ��67   �� *AddNucleotideColor+6m   �� 5�      $��_Remove Symbol��6!   $
��10,5q�6�   $��5 q�6�   ��6:   �� *RemoveNucleotideColor+��6d   �� $ nucList��$ SelectedItem�6�   5�      5�      5�      �� 5�      .R�5�      �  5�      �� 5�      $
��Auto �6(   �� 5�      $
��Auto �6(   �� 5�      $
��Auto �6(   �� 5�      $
��Auto �6(    5�      �� 5�      �� 5�      $��2* �6+   .+�5�      �� $��Center=�6   $��Grouping Minimum:��67   �� 5�      $��1��6   $��Center=�6.   $��75��6I   $��5q�6W   ��6b   �� *MinGroupingRange+$ PropertyChanged=�.��6�    5�        5�        . 6<   .��5�   3   5�      �� 5�      $��1��6   $��2��6,   $��5q�6@   $��Center=�6K   $,��&Display Taxonomy Level in Group Header��6f    6�   �� *ShowTaxonomyGroupLevel+�� 5�      $��2��6   $��2��6,   $��5q�6@   $��Center=�6K   $#��Sort by Name when not Grouped��6f    6�   �� *SortByTaxonomy+�� 5�      $��3��6   $��2��6,   $��5q�6@   $��Center=�6K   $,��&Always open with grouping if supported��6f    6�   �� *OpenWithGrouping+5�      5�      5�       5�   
   �� 5�      $
��Auto �6   $��25��6,   �� 5�       5�   
   �� 5�      �� 5�      $
��Auto �6   .+�5�   
   R� $��10q�6   #��[ 6   
���   �$��1q�6   ��6N   �� *Menu+��5�      �� 5�      ! �� 
TargetType"! Q�6   .�5�      ��  �@Q�
IsExpanded . 63   �� 5       � ��
 �����63   �� 5      �@Q�
IsSelected 63   �� *
IsSelected+" �� Triggers" 5     �� 5      6&   �� *
IsSelected+ True6E   .a�5     ��  � ��$
 Bold"�6;   5     5     5	     �� 5  
   $��1��6   
�������$��10q�6<   ��6H   �� ��5     �� *ResourceKey+# �� 	Converter## [  $ �� FallbackValue#$ [ �� 5  
   $��1��6   $��1��6"   .+�5     �� � 5     $��10,0,0,5q�6    5     �� 5     �� 5     �� 5     .+�5     �� $��OK��6   $��0��6&   $��5q�66   $	��5,0q�6A   % �� 	IsDefault% . $��25��6`   ��6o   �� *Save+ 5     6  0JulMar.Windows.Interactivity.CloseDialogBehavior  5     5     �� 5     $��Cancel��6   $��1��6*   $��5q�6:   $	��5,0q�6E   & �� IsCancel& . 6S   �� 5     $��Reset��6   $��2��6)   $��5q�69   $	��5,0q�6D   ��6R   �� *Reset+5     5     5!     5"     