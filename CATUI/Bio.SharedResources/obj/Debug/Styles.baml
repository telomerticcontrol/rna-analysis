   M S B A M L   `   `   `  ����\  XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089   CloseButton  �� 
TargetType �� BasedOn �� Property �� Value �� 
TargetType � UseLayoutRounding ��Name �� Triggers P� Property	 P� Value
 �� 
TargetName  	TogglePin �Name J� RenderTransformOrigin J� Opacity'    !System.Windows.VisualStateManager    VisualStateGroups%   System.Windows.VisualStateGroup   Transitions%   System.Windows.VisualTransition   GeneratedDuration   States    System.Windows.VisualState   
Storyboard g� 	BeginTime g� Duration �� 
TargetName �� TargetProperty R� KeyTime  System.Windows.Visibility   Visible R� Value O� KeyTime O� Value �� KeyTime �� Value ��Name �� Left
  �� Top�� 5      D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     :x,http://schemas.microsoft.com/winfx/2006/xaml      5      %�  &	        7��&	 �    �� 5       "  ��6!   8   .�5!   
   ��  �@�ToolTip$ 
Close View��6$   �� 5"   
    � �� 5#      �� 5$      " ��6"   .R�5%      � $��16��6   $��16��6&   $��Center=�62   $��Center=�6O    . .+�5&      �� $ bd��6$   #��z��60   #��z��6T   $��1��6z   $y�Hidden=�6�   F� 5'      #��z��6   $��8��6E   $��8��6O   $��Center=�6Z   $��Center=�6w   +����        � z  z �     z � z     	$��2��6�   5(       5)      P� 5*       �@J�IsMouseOver	 . . �5+      �� $
 bd��6K    � y�$ Visible=�6;   6K   �� 5,       � ��
 �����6;   �� 5-       � ��
 �   �6;   5.      50      52      53      �� 55      "  d�6   .�56   
   ��  �@�Cursor$
 Hand��6#   �� 57   
    �@�ToolTip$ Undock View��6$   �� 58   
    � �� 59      �� 5:      " d�6"   .R�5;      � $
 grid��6   $ .5,.5&�6)   # z 6G   z�5<      V� 5=      .~�5>   "   �� �� 5?   "   �� 5@   "   T� 5A   "   5B       5D        5E      CommonStates�� 5F   "     5G   &   $ 00:00:00D�67   . 5I   "     
Normal��6.     5J   "   	MouseOver��. 5K   &   �� .��5L   *   T� $ 00:00:00X�6H   $ 00:00:00.0010000D�6]   $ MouseOverFill��6y   $ (UIElement.Visibility)�6�   ..�5M   .   i� $ 00:00:00��6E   # Z 6X   5N   +   U� 5O   *   $ 00:00:00X�6H   $ 00:00:00.0010000D�6]   $ canvas��6y   $ (UIElement.Opacity)�6�   .\�5P   .   �� $ 00:00:00��6C   $ 1Q�6V   5Q   +   �� 5R   *   $ 00:00:00X�6G   $ 00:00:00.0010000D�6\   $ 	rectangle��6x   $* $(Shape.Fill).(SolidColorBrush.Color)�6�   .g�5S   .   �� $ 00:00:00��6B   $ Black��6U   5T   +   �� 5U   *   $ 00:00:00X�6G   $ 00:00:00.0010000D�6\   $ 
rectangle1��6x   $* $(Shape.Fill).(SolidColorBrush.Color)�6�   .g�5V   .   �� $ 00:00:00��6B   $ Black��6U   5W   +   �� 5X   *   $ 00:00:00X�6G   $ 00:00:00.0010000D�6\   $ 
rectangle2��6x   $* $(Shape.Fill).(SolidColorBrush.Color)�6�   .g�5Y   .   �� $ 00:00:00��6B   $ Black��6U   5Z   +   5[   '   5\   #   5]        5^      CheckStates�� 5_   "     5`   &   $ 00:00:00D�67   . 5b   "     Checked��6.     5c   "   	Unchecked��. 5d   &   �� .��5e   *   U� $ 00:00:00X�6H   $ 00:00:00.0010000D�6]   $
 grid��6y   $V P(UIElement.RenderTransform).(TransformGroup.Children)[2].(RotateTransform.Angle)�6�   .\�5f   .   �� $ 00:00:00��6C   $ 90Q�6V   5g   +   5h   '   5i   #   5j      .+�5l      �� $ MouseOverFill��6$   
�������
���   �$��1��6[   $y�Hidden=�6o   �� 5m      $ canvas��6!   $��8��61   $��12��6;   $
 0.75Q�6G   $��2,4,2,2q�6V   .+�5n      �� $ 	rectangle��6(   
�������$��4��6H   $��6��6R   $ 2��6]   �� 5o      $ 
rectangle1��6(   
�������$��2��6I   $��5��6S   $ 3��6^   $  7��6n   �� 5p      $ 
rectangle2��6(   
�������$��8��6I   $��2��6S   $ 0��6^   $  5��6n   5q      5r       5s      P� 5t       �@d�	IsChecked$	 FalseY�67   . �5u      ��  �@�ToolTip$ 	Dock View��68   5v      5x      5z      5{      5~      