   M S B A M L   `   `   `  ����S  OBio.Controls.ColorPicker, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null+clr-namespace:Bio.ControlsBio.Controls  5          Bio.Controls.ColorPicker   5      \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5      R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      ,localclr-namespace:Bio.Controls     5      .��5      �   � 	Resources  5
   
      ColorSelectBox � ToolTip �� Color  ColorListStyle �� 
TargetType �� BasedOn �� Property �� Value��%�  &	        &	 �     7��7   �� 5      .R�5      �� $��3q�6   
���   �$��1��5       $��10��64   $��10��6?    5   6   �� ��5      �� 5       6*   �� 5      5      �� 5      " ��6+   8   .�5      ��  � ��$ Disabled=�6O   �� 5       � �� 60   Z� �� 5       � �� 5      �� 5      .R�5      � 5      5      �� 5       � ��8  61   -   5        5!      5"       � ColumnDefinitions 5$   
   �� 5%      �� 5&      �� 5'      $��25 �6    � RowDefinitions 5*   
   �� 5+      �� 5,      $
��Auto �6   .+�5/   
   *   $Bio.Controls.ColorSaturationSelector  -   5        	  Name$		 css��5/   3   $��5q�6(   $��1��6@      Bio.Controls.HueSelector  50   
   $��5q�6   $��2��6'   

   Hue
 67   ��  �� ElementName$	 css�� �� Path$	 Hue� �� Mode$ TwoWay=��� 52   
   $��1��6   $��1��6    $��2��60   $��Selected Color��6D   .��53      ��  �� Orientation$ 
Horizontal=�6   .+�54      �� 
���   �$��1��6+   $��50��6?   $��50��6K   $��5q�6V   ��55      �� $	 css��$ 
ColorBrush� J� Effect 56      3  -System.Windows.Media.Effects.DropShadowEffect  57      59      �� 5:      -   5        $	��5,0q�5:      $��Add To Custom Colors��6&   $��Center=�5;      $��5q�64   5<      5=      � 5?   
   $��0��6   $��5q�6   $��2��6*    5@      �� 5A      �� 5B      .+�5E      �� $��Known Colors:��6   .��5F      �� -   5         ��Name$ knownColorsLb��5F      #��[ 61   5G      �� 5I      $��Custom Colors:��6   $��1��6/   .��5J      �� -   5        $ customColorsLb��5J      #��[ 62   5K      5M      5O      5P      