   M S B A M L   `   `   `  ����c  _System.Windows.Interactivity, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35qPclr-namespace:System.Windows.Interactivity;assembly=System.Windows.InteractivitySystem.Windows.Interactivity  5   @    Bio.Views.Alignment.   (Bio.Views.Alignment.Views.FindSearchView   5   	   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   jInteractivityPclr-namespace:System.Windows.Interactivity;assembly=System.Windows.Interactivity     6@   [ WJulmar.Wpf.Behaviors, Version=1.0.0.5, Culture=neutral, PublicKeyToken=ff1c121d8ddc1581Y UJulMar.Wpf.Helpers, Version=1.0.0.5, Culture=neutral, PublicKeyToken=ff1c121d8ddc1581/julmar http://www.julmar.com/wpfhelpers   5   	     � Title$
  Find��5   	    � ShowInTaskbar .   � 
ResizeMode$ NoResize=�6,   $	��300��6B   $	��200��6N    � WindowStartupLocation$ CenterOwner=�5   	   ���	#FF595959 +� FocusedElement 5   	   ��  �� ElementName$ tb��.��5      �  � RowDefinitions 5
   
   �� 5      $
��Auto �6   �� 5      �� 5      $
��Auto �6    � ColumnDefinitions 5   
   �� 5      $
��Auto �6   �� 5      .+�5   
   �� $��_Search For:��6   $��5q�6'   �� 5   
   -   5         ��Name$ tb��5      $��Stretch=�6   $��5q�6<   $��1��6G   ��6W   �� *SearchString+	 �� UpdateSourceTrigger$	 PropertyChanged=�9� 5   
   $��1��6   $��2��6    $��Find Options��64   
 9� 
IsExpanded
 . .��5      �� .+�5      �� $��5q�6   $��Match _whole name��6&    � ToolTip$ Match complete names��6h    d� 	IsChecked 6C   �� *MatchWholeName+6h   �� 5      $��5q�6   $��
Search _up��6&   $) #Search up from the current position��6a    6<   �� *SearchBackward+6a   5      5      �� 5   
   $��2��6   $��1��6"    �� Orientation$ 
Horizontal=�62   $��Right=�6K   .+�5       �� $��
Find _Next��6    �� 	IsDefault . $��5q�6u   $
��10,5q�6�   ��6;   �� *
FindNext+��6X   �� 6�   �� 5!      $��Close��6    �� IsCancel . $��5q�65   $
��10,5q�6@   .   (System.Windows.Interactivity.Interaction   	Behaviors 5"      6  0JulMar.Windows.Interactivity.CloseDialogBehavior  5#         DialogResult .  61   5%      5&      5(      5)      