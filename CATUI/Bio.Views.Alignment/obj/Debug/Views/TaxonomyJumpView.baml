   M S B A M L   `   `   `  ����c  _System.Windows.Interactivity, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35qPclr-namespace:System.Windows.Interactivity;assembly=System.Windows.InteractivitySystem.Windows.Interactivity  5   	    Bio.Views.Alignment/   )Bio.Views.Alignment.Views.TaxonmyJumpView   5      R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   jInteractivityPclr-namespace:System.Windows.Interactivity;assembly=System.Windows.Interactivity     5   	   [ WJulmar.Wpf.Behaviors, Version=1.0.0.5, Culture=neutral, PublicKeyToken=ff1c121d8ddc1581Y UJulMar.Wpf.Helpers, Version=1.0.0.5, Culture=neutral, PublicKeyToken=ff1c121d8ddc1581/julmar http://www.julmar.com/wpfhelpers   5   	   ���	#FF595959.��5      �   � RowDefinitions  5   
   �� 5	      $
��Auto �6   �� 5
      �� 5      $
��Auto �6   .+�5   
   �� $
��Wrap=�6   $��CharacterEllipsis=�6(   
�������$?��9Select the specific Taxonomy to jump to in the list below��5      $��5q�6U   $
��Left=�6`   �� 5   
   $��1��6   
�������$��1q�62   ���	#FF333333$��2q�6]   ._�5      R� -   5         R�Name$ tv��5      $��5q�6@   
���   �$��1q�6_   ��6#   �� *Root+��5      �� 5       �� ItemsSource 6/   �� *
Children+.R�5      ��  �� Orientation$ 
Horizontal=�6%   .+�5      ��  ��Name$
 name��6(   $	��5,0q�6L   $
��Bold"�6Y   
��� �����66   �� *Name+6k   �� 5       ��Name$ count��6)   $ 
Horizontal=�68   .+�5   "   �� $��(��6,   �� 5   "   ��6,   �� *Count+�� 5   "   $��)��6,   5      �� 5      $ 
totalCount��6)   $ 
Horizontal=�6=   $	��5,0q�6V   
������.+�5   "   �� $��(��6,   �� 5   "   ��6,   �� *
TotalCount+�� 5   "   $��)��6,   5       5!       �� Triggers 5"      �� 5#       �� Value$ 0��6D    �� Binding 6*   �� *Count+.a�5$   "   �� 	 �� 
TargetName$	 count��6Q   
 �� Property
 � y� �� Value$ 	Collapsed=�6?   6Q   �� 5%   "   $
	 name��6N   
 � ��$ Normal"�6?   6N   �� 5&   "   $
	 name��6M   
 � ��
 �����5'      �� 5(      $ 0��6I    6*   �� *
TotalCount+.a�5)   "   �� $	 
totalCount��6Q   
 � y�$ 	Collapsed=�6?   6Q   5*      5,      ��5.      �� 5/       �� 
TargetType" Q�6    �� BasedOn# [BQ�66   .�50      �� 
 �@Q�
IsExpanded 67   �� *
IsExpanded+51      53      54      � 56   
   $��2��6     57      �� 58      $
��Auto �6    �� 59      $
��Auto �6     � ColumnDefinitions 5<      �� 5=      �� 5>      $
��Auto �6#   .+�5A      �� $#��Select Rows in Alignment View��6   $��Center=�6?   $��5q�6Z    d� 	IsChecked 6e   �� *SelectRowsInAlignmentView+�� 5C      $ 
Horizontal=�6   $��1��62   .+�5D      �� $��Center=�6   $��5q�63   .��6>   Level:6F   �� 5E      $��5q�6   $	��100��6%   ��64   �� *CollapseLevel+ �� UpdateSourceTrigger$ PropertyChanged=�.��6x   .   (System.Windows.Interactivity.Interaction   	Behaviors 5F      9  3JulMar.Windows.Interactivity.NumericTextBoxBehavior  5G         AllowMouseDrag . 68   .��5H   ;   5I      5J      �� 5K      $��1��6   $��2��6%   $��Close��66    �� IsCancel . $��5q�6V   $
��10,5q�5M      $��Bottom=�6$   $��Right=�6?   ��5K   a   �� *	CloseView+y�5L      �� *
IsDocked+ �� 	Converter <  6JulMar.Windows.Converters.BooleanToVisibilityConverter     TrueTreatment$ 	Collapsed=�   FalseTreatment$ Visible=�5M   ?   5N      �� 5P   
   ���	#FF535353$��3��6(   ��5Q      �� 5R      " ��6   .�5S      �� 
 � y�$ 	Collapsed=�63    �� Triggers 5T      �� 5U       6&   ��  6:   Z� .a�5V      �� 
 � y�$ Visible=�6;   5W      5Y      ._�5[      �� $��Center=�6   $��Center=�61   $��No Taxonomy Data��6L   5\      5^      5_      