   M S B A M L   `   `   `  ����N  JBio.Views.Alignment, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullK*clr-namespace:Bio.Views.Alignment.InternalBio.Views.Alignment.Internal  5#      O,clr-namespace:Bio.Views.Alignment.ViewModelsBio.Views.Alignment.ViewModels  5%      K*clr-namespace:Bio.Views.Alignment.ControlsBio.Views.Alignment.Controls  5&      c _System.Windows.Interactivity, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35qPclr-namespace:System.Windows.Interactivity;assembly=System.Windows.InteractivitySystem.Windows.Interactivity 5'      D @Bio.Views, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullL4clr-namespace:Bio.Views.Utilities;assembly=Bio.ViewsBio.Views.Utilities 5(      /    )Bio.Views.Alignment.Views.AlignmentEditor   5      R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5      R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5       @d2http://schemas.microsoft.com/expression/blend/2008     5!      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5"      ?Internal*clr-namespace:Bio.Views.Alignment.Internal     5#      [ WJulmar.Wpf.Behaviors, Version=1.0.0.5, Culture=neutral, PublicKeyToken=ff1c121d8ddc1581Y UJulMar.Wpf.Helpers, Version=1.0.0.5, Culture=neutral, PublicKeyToken=ff1c121d8ddc1581/julmar http://www.julmar.com/wpfhelpers   5$      ?
ViewModels,clr-namespace:Bio.Views.Alignment.ViewModels   5%      ;Controls*clr-namespace:Bio.Views.Alignment.Controls   5&      fInteractivityPclr-namespace:System.Windows.Interactivity;assembly=System.Windows.Interactivity   5'      F	Utilities4clr-namespace:Bio.Views.Utilities;assembly=Bio.Views   5(      &   System.Windows.Media.TextOptions    TextFormattingMode$  Display=�5)   K   $��Segoe UI, Verdana)�6t   $	��400��5*      $	��600��6    +� FocusedElement 5)      ��  �� ElementName$ LeftAlignmentView�� � 	Resources 5,         headerStyle �� 
TargetType �� BasedOn �� Property �� Value.  (JulMar.Windows.Converters.MultiConverter  doublePlusOneConverter   
Converters8  2JulMar.Windows.Converters.DoubleToIntegerConverter-  'JulMar.Windows.Converters.MathConverter  ScrollbarToolTip	 �� 	Converter
 �� ConverterParameter  DoubleToIntegerConverter<  6JulMar.Windows.Converters.BooleanToVisibilityConverter   BooleanToVisibilityConverter  AlignmentEntityCellTemplate �� DataType=   7Bio.Views.Alignment.ViewModels.AlignmentEntityViewModel0   *Bio.Views.Alignment.Controls.SequenceBlock  Name � UseLayoutRounding   Sequence   IsActive   SequenceColorSelector   Column �� Path �� Mode �� RelativeSource �� AncestorType   Count   FocusedColumnIndex-  'JulMar.Windows.Behaviors.RenderBehavior   
Invalidate �� Triggers �� Value �� Binding �� 
TargetName  rowNumberCellTemplate ��Name  nameCellTemplate �Name ��Name  � ToolTip! � ContextMenu" m� IsCheckable# m� Command$ m� CommandParameter% m� 	IsChecked  RowNumberColumnHeader& �� Width' �� CellTemplate6	  0System.Windows.Controls.GridViewColumnCollection 	 gvMainColumnDefinition3
   -Bio.Views.Alignment.Internal.HeaderRulerIndex( 
  DisplayIncrement) 
  Column* 
  CurrentColumn+ 
  
FontFamily, 
  FontSize 
 gvRightColumnDefinition  LeftItemTemplate- �� Columns. �� Content  RightItemTemplate  ItemDragTemplate  GroupItemTemplate/ � ColumnDefinitions  ShadedItemTemplate.  (Bio.Views.Alignment.Internal.RowSelector  MainGroupOrItemSelector0   GroupTemplate1   SequenceTemplate  RightGroupOrItemSelector  NucelotideListBoxStyle2 �� 
TargetType3 �� Triggers4 ��Name5 J� SnapsToDevicePixels6 �� Padding7 ��Name8 � RowDefinitions9 ��Name: �� CanContentScroll; �� CanHorizontallyScroll< �� CanVerticallyScroll= ��Name> � Cursor*? �� #ComputedVerticalScrollBarVisibility@ � MinimumA �� ScrollableHeightB � MaximumC �� ViewportHeightD �� ViewportSizeE � Value.  (System.Windows.Interactivity.InteractionF   	Behaviors;  5JulMar.Windows.Interactivity.ScrollbarPreviewBehaviorG   ScrollingPreviewTemplateH �� TriggersI P� PropertyJ P� Value  GridViewHeaderStyle��%�!  &	        7��&	 t     &	 �     7  &	     &	 +    &	 >    &	 �    &	 �    &	 �    7  &		 �    7  7  &	
 >    7  &	 �    7 	 &	 �    7 
 &	 	    &	 �    7  &	 �    7  &	 d    7  7  &	 �    7  7  &	 �    7��7  &	      7   �� 5-   
   " ��6$   8   .�5.      ��  � ��$
 Left=�6;   �� 5/       � ��$	 2,1q�60   50        52   
   . 53          54      55      �� 57   
   .R�58      �� 
�������$
��Bold"�6�   ��6   �� *Value+8	   $
 +1��6�   59        5;   
   6*     5<   
   6.   �� 5>   
   "  6;   .R�5?        $ sequence��6%    . $��Center=�5H   >    5@   %   �� *AlignedData+ 6F   �� *	IsFocused+ 5A   %   �� * Parent.NucleotideColorSelector+��5B   %   �� *$"Parent.Options.AlignmentFontFamily+��5C   %   �� *%#Parent.Options.AlignmentFontSizeDpi+ 5D   %   �� $ Tag.FirstColumn�$ TwoWay=� �� *FindAncestor+" �� 5E   %   �� $ Tag.VisibleColumns�$ OneWayToSource=� �� *FindAncestor+" �� 5F   %   ��  �� *FindAncestor+" ��$ Tag.FocusedColumnIndex�$ TwoWay=� 5G   %   �� *Parent.LockedRows.Count+5H   >    5I      �� 5J      $
 True��6=    6   �� *
IsSelected+.a�5K      �� $ sequence��6    6I   �� *Parent.Options.SelectionBrush+ � ���� 5L      $ sequence��6    6J   �� *%#Parent.Options.SelectionBorderBrush+ � ��5M      �� 5N      $
 True��6<    6   �� *	IsFocused+.a�5O      �� $ sequence��6    6I   �� *Parent.Options.FocusedBrush+ � ���� 5P      $ sequence��6    6J   �� *#!Parent.Options.FocusedBorderBrush+ � ��5Q      �� 5R      $
 True��6F    6   �� *IsReferenceSequence+.a�5S      �� $ sequence��6    6I   �� *ReferenceSequenceColor+ � ���� 5T      $ sequence��6    6J   �� *ReferenceSequenceBorder+ � ��5U      5W      �� 5Y   
   .R�5Z      � -   5        .+�5[      �� $ tb��6   $
��Left=�5\   V   $��Center=�6q   ��5[   (   �� *DisplayIndexText+��6J   �� *Parent.Options.SelectedFont+��6}   �� *$"Parent.Options.SelectedFontSizeDpi+��5\      �� *$"Parent.Options.LineNumberTextBrush+6q   �� 5]      $��1��6   $��Stretch=�6&   $
��Left=�6B   
�������$��-5,0,0,0q�6m   �� 5^      $��1��6   $��Stretch=�6&   $��Right=�6B   
�������$��0,0,-5,0q�6n   5_      5`      �� 5b   
   .R�5c      � $
 grid��6   -   5        .+�5d      �� $ bd��6   $y�	Collapsed=�6(   $��1��6�   ��6?   �� *Parent.Options.SelectionBrush+��6n   �� *%#Parent.Options.SelectionBorderBrush+6�   �� 5e      $
 text��6   $��CharacterEllipsis=�6B   ��6*   �� *Header+��6c   �� *$"Parent.Options.ForegroundTextBrush+��5f      �� *Parent.Options.SelectedFont+��6O   �� *$"Parent.Options.SelectedFontSizeDpi+�� 5g      $��1��6   $��Stretch=�6&   $
��Left=�6B   
�������$��-5,0,0,0q�6m   �� 5h      $��1��6   $��Stretch=�6&   $��Right=�6B   
�������$��0,0,-5,0q�6n     5j      �� 5k      ��6    �� *Header+  68   �� *Header+! 5n      �� 5o      .@�5p      m� $��Reference Sequence��6#   $	��5,4q�6p   " . # 6?   �� *Parent.MakeReferenceSequence+$ 5q   /   �� % 6_   �� *IsReferenceSequence+$ OneWay=�5r      5t       5u      �� 5v      $
 True��6=    6   �� *
IsSelected+.a�5w      �� $ bd��6    � y�$ Visible=�6C   �� 5x      $
 text��6    6E   �� *#!Parent.Options.SelectionTextBrush+ � ��5y      5{      �� 5}   
   $��#��67   $& 60��6B   8'   6M   	  5   
   �� 5�      $
��Name��6   $
& Auto��6+   8'   68   �� 5�      $
& Auto��6   8'  ��5�      
  5�      $( 10��5�   0   $��1,0,0,0q�6F   ) 5�   0   �� *LeftView.FirstColumn+$ TwoWay=�* 6e   �� *FocusedColumnIndex++ 5�   0   �� *Options.AlignmentFontFamily+, 6c   �� *Options.AlignmentFontSizeDpi+5�   F   5�      5�      	  5�   
   �� 5�      $
& Auto��6   8'   ��5�      
  5�      $( 10��5�   0   $��1,0,0,0q�6F   ) 5�   0   �� *RightView.FirstColumn+$ TwoWay=�* 6f   �� *FocusedColumnIndex++ 5�   0   �� *Options.AlignmentFontFamily+, 6c   �� *Options.AlignmentFontSizeDpi+5�   F   5�      5�      �� 5�   
   .R�5�      �� 8-   . 6#   �� 67   5�      �� 5�   
   .R�5�      �� 8-   . 6#   �� 67   5�      �� 5�   
   .R�5�      �� 
���   �$��2q�6)   ._�5�      �� 
�������$��1q�6-   ���	#FF353535._�5�      �� 
�������$��5q�6K   ��6    �� *Header+6K   5�      5�      5�      �� 5�   
   .R�5�      � ��6   �� *!Parent.Options.GroupHeaderBrush+  6J   �� *
TaxonomyId+/ 5�      �� 5�      $
��Auto �6%   �� 5�      .+�5�      �� $��5q�6   $
��Bold"�6�   ��6'   �� *TaxonomyGroupLevel+��6K   �� *%#Parent.Options.GroupHeaderTextBrush+��5�      �� *Parent.Options.SelectedFont+��6O   �� *$"Parent.Options.SelectedFontSizeDpi+y�5�      �� *'%Parent.Options.ShowTaxonomyGroupLevel+8	   �� 5�      $��1��6   $��5q�6,   ��67   �� *Header+��6O   �� *%#Parent.Options.GroupHeaderTextBrush+��5�      �� *Parent.Options.SelectedFont+��6O   �� *$"Parent.Options.SelectedFontSizeDpi+5�      5�      �� 5�   
   .R�5�      � ��6   �� *!Parent.Options.GroupHeaderBrush+  6J   �� *
TaxonomyId+/ 5�      �� 5�      $
��Auto �6'   �� 5�      .+�5�      �� $��5q�6   $
��Bold"�6�   ��6'   �� *TaxonomyGroupLevel+��6K   �� *!Parent.Options.GroupHeaderBrush+��5�      �� *Parent.Options.SelectedFont+��6O   �� *$"Parent.Options.SelectedFontSizeDpi+y�5�      �� *'%Parent.Options.ShowTaxonomyGroupLevel+8	   �� 5�      $��1��6   $��5q�6,   ��67   �� *Header+��6O   �� *!Parent.Options.GroupHeaderBrush+��5�      �� *Parent.Options.SelectedFont+��6O   �� *$"Parent.Options.SelectedFontSizeDpi+5�      5�        5�   
   80   81  6r     5�   
   80   81  6t   �� 5�   
   " ��6/   8   .�5�      ��  �@��SelectionMode$ Extended=�6.   �� 5�       � ��$ Disabled=�6K   �� 5�       � �� 6+   �� *Options.BackgroundBrush+�� 5�       � ��$
 Auto=�6I   �� 5�      " �@��IsDeferredScrollingEnabled . 6H   �� 5�       �@$�VirtualizationMode$ 	Recycling=�6J   �� 5�       �@�UseLayoutRounding . 62   �� 5�       � �� 5�      �� 5�      " ��6   .�5�      ��  �@��
IsSelected 67   �� *
IsSelected+$ TwoWay=��� 5�       �@�FocusVisualStyle 5�      �� 5�   "   .�5�   &   ��  � �� 5�   *   �� 5�   .   5�   '   5�   #   5�      �� 5�       � �� 5�      �� 5�   "   "2 ��62   .R�5�   &   �� $��0q�67   5�   #   5�      3 5�      �� 5�       6*   �� *IsGroupHeader+ True6L   .a�5�   "   ��  � |� .  6H   5�      5�      5�      �� 5�       � �� 5�      �� 5�      "2 ��6&   .R�5�      �� $4 Bd��6!   5 . #��z��6H   #��z��5�   !   #��z��6M   $6 1q�6�   ._�5�      �� $7 PART_ScrollViewer��6+   #��z��6F   |�.  ��5�   "   �� 5�   &   "2 ��66   .R�5�   *   � $
 Grid��6/   #��z��6=   / 5�   .   �� 5�   2   $��* �6C   �� 5�   2   $
��Auto �6C   8 5�   .   �� 5�   2   $��* �6@   �� 5�   2   $
��Auto �6@   .+�5�   .   �� $ Corner��68   #��� ��6H   $��1��6�   $��1��6�   �� 5�   .   $!9 PART_ScrollContentPresenter��6E   #��z��6j   $��0��6�   $��0��6�   #��z��5�   E   #��z��6i   #: z��5�   E   ; .  < .  6�   �� 5�   .   $= PART_VerticalScrollBar��68   $> Arrow��6X   $��1��6g   $��0��6w   #y�z? 5�   6   $@ 0Q�5   6   #B zA 6B   #D zC 5  6   E 5  6   �� *VerticalOffset+$ OneWay=� �� *TemplatedParent+F 5  0     5  2   8G  6R   5  -   5  +   5  '   .��5
  "   �� #5 z5 61   5     5     H 5     P� 5     I � {�J .  . �5  "   �� $ Bd��6?   # � ��6O    � ��5     P� 5     I �@��
IsGroupingJ . . �5  "   ��  � �� .  6R   5     5     5     5     �� 5  
   " ��6,   .�5     ��  � �� 65   �� *Options.SelectedFontSizeDpi+�� 5      � �� 67   �� *Options.SelectedFont+�� 5     " �@��ColumnHeaderContainerStyle8   6;   5     5      K J� InputBindingsK 5"     �� 5#  
   
L �� Key$L C��6   M �� 	Modifiers$M Controlf�6   N �� CommandN 61   �� *
GotoColumn+�� 5$  
   $L R��6   $M Controlf�6   N 61   �� *	GotoRow+�� 5%  
   $L N��6   $M Controlf�6   N 61   �� *GotoRefColumn+�� 5&  
   $L O��6   $M Controlf�6   N 61   �� *ShowOptionsCommand+�� 5'  
   $L T��6   $M Controlf�6   N 61   �� *FlipGrouping+�� 5(  
   $L F��6   $M Controlf�6   N 61   �� *
FindByName+�� 5)  
   $L T��6   $M Controlf�6   N 61   �� *FindByTaxonomy+�� 5*  
   $L S��6   $M Controlf�6   N 61   �� *SplitScreenCommand+�� 5+  
   $L B��6   $M Controlf�6   N 61   �� *ShowBirdsEyeView+�� 5,  
   $L R��6   $	M Altf�6   N 6-   �� *MakeReferenceSequence+O �� CommandParameterO 6W   �� *
FocusedRow+�� 5-  
   $L F3��6   N 6   �� *JumpNextNucleotide+�� 5.  
   $L F2��6   N 6   �� *JumpPreviousNucleotide+�� 5/  
   $L F3��6   $M Controlf�6   N 62   �� *JumpNextRefNucleotide+�� 50  
   $L F2��6   $M Controlf�6   N 62   �� *JumpPreviousRefNucleotide+�� 51  
   $L PageDown��6   $M Controlf�6$   $ N ScrollBar.PageRightCommand��68   �� 52  
   $L PageUp��6   $M Controlf�6"   $N ScrollBar.PageLeftCommand��66   �� 53  
   $
L Home��6   $M Controlf�6    $&N  ScrollBar.ScrollToLeftEndCommand��64   �� 54  
   $	L End��6   $M Controlf�6   $'N !ScrollBar.ScrollToRightEndCommand��63   �� 55  
   $
L Left��6   N 6    �� *ShiftFocusLeft+�� 56  
   $L Right��6   N 6!   �� *ShiftFocusRight+.��59     ]� .+�5:  
   �� $��Bottom=�6   .@�5;     d� $  Split Screen View��6   P d� 	IsCheckedP 66   �� *IsScreenSplit+$ OneWay=���6g   �� *SplitScreenCommand+.��5<     � $��16��6   $��16��6   .+�5=     �� 
�������$��2��6)   �� 5>     
�������$��1��6)   $��1��6=   $��12��6G   5?     5@     d� 5A     $  Show Birds Eye View��6   P 68   �� *IsBevShowing+$ OneWay=���6h   �� *ShowBirdsEyeView+.��5B     � $��16��6   $��16��6"   .+�5C     A� 
�������$��14��6-   $��10��68   A� 5D     
�������$��5��6+   $��5��65   5E     5F     d� 5G     $  Turn Grouping On and Off��6   P 6>   �� *	IsGrouped+$ OneWay=���6k   �� *FlipGrouping+.��5H     � $��16��6   $��16��6"   .+�5I     F� 
�������s���Ѐ� �� р� ��� Ѐ� ��- р�� ��- Ѐ� ��[ р�� ��[ Ѐ�  z � z  z Ѐ� ��� �@T� ��� Ѐ�  � р��  � 	Q �� Stretch$
Q Fill=�6u   5J     5K     �� 5L     $  Change Visual Options��6<   $��4q�6\   ��6   �� *ShowOptionsCommand+.��5M     � $��16��6   $��16��6"   .+�5N     A� 
�������$��1��6-   �� 5O     $��i��6    
�������$��10(�6<   $
��Bold"�6J   $��Center=�6\   $��Center=�6y   5P     5Q     � 5S     / 5T     �� 5U     �� 5V     $��15 �6'   .+�5X     �� $��2��6   $��-1q�6/   $��Right=�6;   $��20��6W   .@�5Y     �� $��Goto Column...��6#   R   TriggersR 5Z     /  )JulMar.Windows.Interactivity.ClickTrigger  5[     .  (System.Windows.Interactivity.TriggerBaseS   Actions.S 5\  "   *  $JulMar.Windows.Actions.InvokeCommand  T   CommandT 67   �� *
GotoColumn+5]     5_     �� 5`     $ ��Goto Reference Sequence...��6#   {�6H   �� *HasReferenceSequence+R 5a       5b     .S 5c  "     T 67   �� *GotoRefColumn+5d     5f     �� 5g     $��Goto Row...��6#   R 5h       5i     .S 5j  "     T 67   �� *	GotoRow+5k     5m     �� 5n     $��Find Name...��6#   R 5o       5p     .S 5q  "     T 67   �� *
FindByName+5r     5t     �� 5u     $��Taxonomy Browser...��6!   {�6?   �� *	IsGrouped+R 5v       5w     .S 5x       T 6/   �� *FindByTaxonomy+5y     5{     5|     �� 5}     $  
Jump To...��68   $��4q�6M   $��0q�6Y   ��6   �� *
GotoColumn+.��5~     �� $��16��6   $��16��6(   .+�5     A� 
�������$��1��61   $��12��6E   $��12��6P   �� 5�     
�������$��1��6.   	U �� X1$U 10��6B   	V �� X2$V 16��6J   	W �� Y1$W 10��6R   	X �� Y2$X 16��6Z   5�     5�     5�     �� 5�     $��Stretch=�6   $
��Left=�64   $��1��6O   �� 5�     $��Right=�6   .��5�     �� Y �� Orientation$Y 
Horizontal=�6   .+�5�     �� $Y 
Horizontal=�6!   $
��Bold"�6�   y�6:   �� *HasReferenceSequence+#	 [ .+�5�     �� $��Stretch=�6#   $��1��6?   �� 5�     $��[��6#   $��6,0,3,0q�6,   �� 5�     ��6#   �� *FocusedColumnReferenceIndex+�� 5�     $��]��6#   $��3,0,6,0q�6,   5�     �� 5�     $��Stretch=�6    $��1��6<   �� 5�     $��6,0,0,3q�6    ��61   �� *FocusedColumnIndex+	   $
 +1���� 5�     $��3,0,0,3q�6    $	�� / ��61   �� 5�     $��3,0,0,3q�6    ��61   �� *TotalColumns+�� 5�     $��3,0,0,3q�6    $�� cols, ��61   �� 5�     $��6,0,0,3q�6    ��61   �� *FocusedRowDisplayIndex+Z �� TargetNullValue$Z ?���� 5�     $��3,0,0,3q�6    $	�� / ��61   �� 5�     $��3,0,0,3q�6    ��61   �� *TotalEntities+�� 5�     $�� rows��6    5�     5�     5�     � 5�  
   8 5�     �� 5�     $
��Auto �6    �� 5�     $
��Auto �6    �� 5�     $
��Auto �6    �� 5�     �� 5�     $
��Auto �6    / 5�     �� 5�     �� 5�     $
��Auto �6!   �� 5�     ��6!   �� *RightSidePaneSize+	 :   4Bio.Views.Alignment.Internal.DoubleToGridColumnWidth  $ TwoWay=�.+�5�     �� -   5        [ ��Name$[ 
LeftHeader��5�  )   #��[ 6=   #- [	 5�  )   �� 5�     $��1��6   #��[ 5�      . (  "Bio.Views.Utilities.DragDropHelper\   IsDropTarget\ . ]   IsDragSource] . ^   DragDropTemplate#^ [ 5�     ��5�  (   �� *
LockedRows+��6K   �� *Options.BackgroundBrush+
_ � Tag_ 5�  ^   �� *
LeftView+5�     �� 5�     $	��0,2q�6   $��10��6$   $��2��60   $
Q Fill=�6=   
�������$��1��6[   $��3��6o   ��5�     �� *Options.SeparatorBrush+y�6>   �� *HasLockedRows+#	 [ �� 5�     -   5        ` ��Name$` LeftAlignmentView��5�     $��3��61   #��[ 6>   #��[ 5�     _ 5�  n   �� *
LeftView+��5�     �� *VisibleData+a �� SelectedItema 6:   �� *
FocusedRow+��5�     �� *IsScreenSplit+	 H  BBio.Views.Alignment.Internal.BooleanToScrollBarVisibilityConverter  F 5�     ?  9JulMar.Windows.Interactivity.ViewportSynchronizerBehavior  5�     b   VerticalOffsetb 68   �� *TopRow+c   ViewportHeightc 5�  8   �� *VisibleRows+5�     �� 5�     -   5        $= hsbLeft��5�     $��4��6)   d �� Orientation$d 
Horizontal=�66   $
��Left=�6O   $> Arrow��6�   $@ 0Q�5�     e � SmallChange$e 1Q�5�     f � DataContextf 5�     �� *
LeftView+E 69   �� *FirstColumn+$ TwoWay=�B 5�  $   �� *NotVisibleColumns+#	 [ D 5�     �� *VisibleColumns+#	 [ g � LargeChangeg 5�  (   �� *VisibleColumns+#	 [ F 5�       5�     #G [ 62   5�     �� 5�     $��2��6   
���   �$��Stretch=�68   $��Center=�6T   $��5��5�     $��1��6,   h �� ResizeBehavior$h PreviousAndNext=�6<   y�5�     �� *IsScreenSplit+#	 [ �� 5�     -   5        $[ RightHeader��5�  )   $��2��6>   #��[ 6N   #- [
 5�  )   �� 5�     $��1��6   $��2��6(   #��[ 5�      . ��5�  8   �� *
LockedRows+��6[   �� *Options.BackgroundBrush+_ 5�  _   �� *	RightView+�� 5�     -   5        $` RightAlignmentView��5�     $��3��62   $��2��6?   #��[ 6O   #��[ 5�     _ 5�     �� *	RightView+��60   �� *VisibleData+a 6T   �� *
FocusedRow+F 5�       5�     b 6:   �� *TopRow+5�     �� 5�     -   5        $= hsbRight��5�     $��4��6*   $��2��67   $d 
Horizontal=�6G   $
��Left=�6`   $> Arrow��6�   $@ 0Q�5�     $e 1Q�5�     f 5�     �� *	RightView+E 6:   �� *FirstColumn+$ TwoWay=�B 5�  $   �� *NotVisibleColumns+#	 [ D 5�     �� *VisibleColumns+#	 [ g 5�  (   �� *VisibleColumns+#	 [ F 5�       5�     #G [ 60   5�     5�     5�     5�     