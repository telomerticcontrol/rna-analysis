   M S B A M L   `   `   `  ����  Bio.Data.Providers.rCAD.RI8    2Bio.Data.Providers.rCAD.RI.Resources.DataTemplatesR NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35[ WJulmar.Wpf.Behaviors, Version=1.0.0.5, Culture=neutral, PublicKeyToken=ff1c121d8ddc1581Y UJulMar.Wpf.Helpers, Version=1.0.0.5, Culture=neutral, PublicKeyToken=ff1c121d8ddc1581R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089   TaxonomyData  �� ItemsSource �� 
TargetType �� Triggers �� Binding �� Value �� Property �� Value  FilterSelection � RowDefinitions � ColumnDefinitions	 �� DisplayMemberPath
 �� SelectedValuePath �� SelectedValue �� Mode R� SelectedValuePath-  'JulMar.Windows.Behaviors.EventCommander   Mappings+  %JulMar.Windows.Behaviors.CommandEvent   Event   Command �� BasedOn �� Orientation �� IsAsync �� FallbackValue �� 	Converter7  1JulMar.Windows.Converters.BooleanToBrushConverter   	TrueBrush   
FalseBrush  DatabaseSelection �� ValidatesOnDataErrors �� SelectedItem 	� 	GroupName d� 	IsChecked<  6JulMar.Windows.Converters.BooleanToVisibilityConverter   TrueTreatment   FalseTreatment   5      D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5      /julmar http://www.julmar.com/wpfhelpers   5      :x,http://schemas.microsoft.com/winfx/2006/xaml     5       %�  &	        &	 >    7   7Q�&	 -
    �� 5"        64   �� *
Children+.R�5#   
   �� $	��1,5q�6   ��6!   �� *Name+��5$      �� 5%      " ��6    5&      �� 5'       6&   �� *
IsSelected+ True6E   .a�5(      ��  � ��$
 Bold"�6;   �� 5)       � ��
 �   �6;   5*      5,      5.      5/      �� 51      .R�52   
   ]� .+�53      �� $��5q�6   $	��Top=�6    ._�54      �� $��5q�6   �� 55      .�6   	Step 2:6     �� 56      .�6   :8Select the filter criteria to locate sequence alignments6P   57      58      �� 5:      $��Filters��6   $��5q�6(   $	��Top=�63   .��5;      �  5<      �� 5=      $
��Auto �6(   �� 5>      $
��Auto �6(   �� 5?      $
��Auto �6(    5B      �� 5C      $
��Auto �6+   �� 5D      .+�5G      �� $��Sequence Type:��6   $��5q�65   $��Right=�6@   $��Center=�6\   �� 5H      $��1��6   $��Stretch=�6/   $��5q�6M   $��Center=�6X   $
	 Name��5I   A   $
 Id��5J      ��5I      �� *SequenceTypes+ 5J   2   �� *SequenceTypeId+$ TwoWay=��� 5L      $��1��6   $��
Alignment:��6)   $��5q�6>   $��Right=�6I   $��Center=�6e   �� 5M      $��1��6   $��1��6,   $��Stretch=�6<   $��5q�6Z   $��Center=�6e   $
	 Name��5N   >   $
 Id��5O      ��5N      �� *
Alignments+ 5O   2   �� *AlignmentId+$ TwoWay=��� 5Q      $��2��6   $��Cell Location:��6)   $��5q�6B   $��Center=�6M   �� 5R      $��2��6   $��1��6,   $��Stretch=�6<   $��5q�6Z   $��Center=�6e   $
	 Name��5S   E   $
 Id��5T      ��5S      �� *CellLocationTypes+ 5T   2   �� *
LocationId+$ TwoWay=�5U      5V      �� 5X      $��Taxonomy Selection��6   $��5q�63   .��5Y      �  5Z      �� 5[      �� 5\      $
��Auto �6(   .+�5_      R� $��5q�6   $ Id��5`      $��0��66   8��  ��5_   *   �� *TaxonomyRoot+ 5c        $ SelectedItemChanged��62    6N   �� *SelectTaxonomyCommand+��5e      �� 5f      " Q�6$   8  .�5g   "   ��  �@Q�
IsExpanded 6?   �� *
IsExpanded+$ TwoWay=��� 5h   "    �@Q�
IsSelected 6?   �� *
IsSelected+$ TwoWay=�5i      5k      �� 5m      $��1��6!   $ 
Horizontal=�6.   .+�5n      �� $,��&Sequences found in taxonomy selection:��6    $��5q�6Q   �� 5o      $��5q�6r   $
��Bold"�6}   ��6    �� *MatchingSequenceCount+ . $ 
(counting)����6�   �� *HasValidSequenceList+   
 � ���
 �  ��5p      5r      5s      5u      5v      �� 5x      .R�5y   
   ]� ��.  .+�5z      �� $��5q�6   $	��Top=�6    ._�5{      �� $��5q�6   �� 5|      .�6   	Step 1:6     �� 5}      .�6   +)Enter the database connection information6A   5~      5      �� 5�      $��Connection Name��6   $��5q�60   $	��Top=�6;   .��5�      �� $��5q�6   ��6%   �� *Name+ . 5�      �� 5�      $��Server��6   $��5q�6'   $	��Top=�62   .��5�      �� $��5q�6   ��6%   �� *Server+ . 5�      �� 5�      $��Database��6   $��5q�6)   $	��Top=�64   .��5�      �� $��5q�6   ��6&   �� *DatabaseNames+ .  5�      �� *
Database+ .  5�        $ DropDownOpened��6.    6E   �� *FillDatabaseCommand+5�      5�      �� 5�      $��Logon Details��6   $��5q�6.   $	��Top=�69   .��5�      �� .+�5�      	� $��5q�6"   $ SecurityType��6-   $��Windows Authentication��6n    6F   �� *IsWindowsSecurity+6n   	� 5�      $��5q�6"   $ SecurityType��6-   $��SQL Authentication��6p    6F   �� *IsSqlServerSecurity+6p   � 5�      $��20,0,0,0q�6   y�6-   �� *IsSqlServerSecurity+   $ Visible=�$ 	Collapsed=� 5�      �� 5�      $
��Auto �6,   �� 5�      $
��Auto �6,    5�      �� 5�      $
��Auto �6/   �� 5�      .+�5�      �� $��5,0,5,5q�6    $��	Username:��61   $��Right=�6E   �� 5�      $��1��6"   $��Stretch=�6L   $��Center=�6j   $	��5,0q�6�   ��62   �� *
Username+6�   �� 5�      $��1��6    $��5q�6-   $��	Password:��68   $��Right=�6L   �� 5�      $��1��6"   $��1��6/   $��Stretch=�6Y   $��Center=�6w   $	��5,0q�6�   ��6?   �� *
Password+6�   5�      5�      5�      �� 5�      $��Connection String��6   $��5q�62   $	��Top=�6=   .��5�      �� $
��Left=�6   $
��Wrap=�67   $��5q�6K   
�������$	��300��6i   ��5�      �� *ConnectionString+$ (none)��5�      �� 5�      $��5q�6   $��Right=�6"   $	��Top=�6?   $��Remember this connection��5�       5�      �� *ShouldSerialize+5�      5�      5�      