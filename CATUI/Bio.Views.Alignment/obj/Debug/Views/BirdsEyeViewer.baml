   M S B A M L   `   `   `  ����Y  UJulMar.Wpf.Helpers, Version=1.0.0.5, Culture=neutral, PublicKeyToken=ff1c121d8ddc1581aCclr-namespace:JulMar.Windows.Converters;assembly=JulMar.Wpf.HelpersJulMar.Windows.Converters  5   K    Bio.Views.Alignment.   (Bio.Views.Alignment.Views.BirdsEyeViewer   5      R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5      R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      VjulmarCclr-namespace:JulMar.Windows.Converters;assembly=JulMar.Wpf.Helpers     6K   ���	#FF595959  � UseLayoutRounding  . .��5      � -   5         �Name$ rootGrid��5      .+�5   
   (�  (� Stretch$
 Fill=�6   .��5	      �� $��5q�6   ��6    ��  �� ElementName$	 img�� �� Path$ ActualWidth���6T   �� $	 img��$ ActualHeight�.+�5
      �� -   5         ��Name$	 img��5
      $
��None=�6A   $
��Left=�6P   $	��Top=�6k    �� BitmapScalingMode$ NearestNeighbor=�5       �� EdgeMode$ Aliased=�6L   ��5
   %   �� *
BevImage+5   L   �� 5      -   5         ��Name$ rcBox��5      
���   �$��1��6:   $
��Left=�6O   $	��Top=�6j   ���	#90B9D3E2	 � Cursor$
	 Hand��5   [   ��6   �� *VisibleColumns+��6<   �� *VisibleRows+
 �� Left
 6i   �� *FirstColumn+
 �� Top 6�   �� *TopRow+5      5      � 5   
   -   5         �Name$ positionText��5      $��Center=�6W    � PlacementTarget 6&   �� $ rootGrid��.��5      �� 
���   �
�������$��1q�6<    J� Effect 5      3  -System.Windows.Media.Effects.DropShadowEffect  5      ._�5      �� 
���   �$��5q�6M   ��6/   �� *PositionText+6M   5      5      � 5   
   y�6   �� *IsRendering+ �� 	Converter <   6JulMar.Windows.Converters.BooleanToVisibilityConverter  .+�5      �� ���	#900000006   �� 5      $��Center=�6   $
��12pt(�65   
���   �$��Center=�6X   $��Generating View...��6s   �� 5      $��2,2,0,0q�6   $��Center=�6)   $
��12pt(�6F   
�������$��Center=�6i   $��Generating View...��6�   5      5       5!      