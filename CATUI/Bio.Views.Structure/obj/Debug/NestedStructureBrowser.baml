   M S B A M L   `   `   `  ����N  JBio.Views.Structure, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullO,clr-namespace:Bio.Views.Structure.ViewModelsBio.Views.Structure.ViewModels  5       0    *Bio.Views.Structure.NestedStructureBrowser   5      R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5      R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      C
ViewModels,clr-namespace:Bio.Views.Structure.ViewModels     5         � 	Resources  5!       �� DataType?  9Bio.Views.Structure.ViewModels.NestedElementTickViewModel �Name �� Path � RowDefinitions	 �� X1	 �� X2	 �� Y1	 �� Y2	 �� Stretch;  5Bio.Views.Structure.ViewModels.NestedElementViewModel
 F�Name � ToolTip �� Orientation �� Triggers �� Value �� Binding �� 
TargetName �� Property �� Value<  6Bio.Views.Structure.ViewModels.NestedBasePairViewModel��%  (��       5"      *, +)(��     5<      *, +)(�� �    5N      *, +)�� 5"   
   "  6   .R�5#      � $
 Tick��6   ��5$      �� $ 	TickWidth� 5%      �� 5&      $��10 �6$   62   �� 5'      $��* �6$   61   .+�5)      �� $��0��6   $��0��5*      $ 0��5+      $ 0��5,      $ 0��5-      $ 1��5.      $
	 Fill=�5/      
���   �$��1��51      $��Center=�52      53      �� 54      $��1��6   $��0��55      $��Center=�57      $	��8pt(�58      $��Courier New)�59      ��56      �� $ TickText�59   7   5:      5;      �� 5<   
   "  6   .R�5=      F� $
 
lineElemNt��6   
���   �$��6��5?      ��5@      �� $ LinePath� 5A      �� 5B      $ 
Horizontal=�6!   .+�5C      �� Nt:6*   �� 5D      ��6$   �� $ Index�6B   5E      5G       5H      �� 5I      $
 True��6B    6   �� $ 
IsSelected�.a�5J      �� $ 
lineElemNt��6/    � ��
 �  ��6U   5K      5M      �� 5N   
   "  6   .R�5O      F� $
 lineBP��6   
���  ��$	��1.5��5Q      ��5R      �� $ 	Connector�65    5S      �� 5T      $
 True��6B    6   �� $ 
IsSelected�.a�5U      �� $ lineBP��6/    � ��
 ����6[   5V      5X      5Y      .��5Z      ]� .+�5[   
   �� $	��Top=�6   .@�5\      �� .��5]      �� $ 
Horizontal=�6   .+�5^      �� Nesting Diagram:62   �� 5_      ��6    ��  �� ElementName$ lb��$ SelectedItem�6U   5`      5a      �� 5b      $��Right=�6   .��5c      �� $ 
Horizontal=�6   .+�5d      �� Zoom:6'   �� 5e      -   5         ��Name$ 
viewScaler��5e       � Value$ 1Q�61    � Minimum$ .5Q�6;    � Maximum$ 10Q�6H   $	��100��6U    �� TickFrequency$ .1Q�5f       �� TickPlacement$ TopLeft=�6,    �� Orientation$ 
Horizontal=�6D   6_   5g      5h      5i      �� 5j   
   -   5         ��Name$ lb��5j      ��5k      �� $ ModelElements���5l      Z� ��5m      �� 5n       �� 
TargetType" ��6   .�5o      ��  �@��Top 63   �� $ Y�6N   �� 5p       �@��Left 64   �� $ X�6O   �� 5q       �@��
IsSelected 63   �� $ 
IsSelected� �� Mode$ TwoWay=�6d   �� 5r       � �� 5s      �� 5t       �� 
TargetType" ��6.   .R�5u   "   �� #��z��63   #��z��5v   3   5w   #   5x      5z      5{      ��5}      �� 5~      .R�5      �� ��6   �� $ Width���5�      �� $ Height� � LayoutTransform 5�      V� 5�      .~�5�   "   ��   �� ScaleX  61   �� $ 
viewScaler��$ Value�! �� ScaleY! 5�   1   �� $ 
viewScaler��$ Value�" �� CenterX" 5�   1   �� # �� RelativeSource# �� *FindAncestor+$ �� AncestorType"$ ��$ ContentHorizontalOffset�% �� CenterY% 5�   1   �� # �� *FindAncestor+"$ ��$ ContentVerticalOffset�6�   5�      5�      5�      5�      5�      5�      