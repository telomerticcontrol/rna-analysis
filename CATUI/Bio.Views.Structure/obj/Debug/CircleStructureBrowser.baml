   M S B A M L   `   `   `  ����R  NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R8clr-namespace:System.ComponentModel;assembly=WindowsBaseSystem.ComponentModel  5       N JBio.Views.Structure, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullO,clr-namespace:Bio.Views.Structure.ViewModelsBio.Views.Structure.ViewModels 5!      0   *Bio.Views.Structure.CircleStructureBrowser   5      W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5      R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml      5      Hscm8clr-namespace:System.ComponentModel;assembly=WindowsBase      5       C
ViewModels,clr-namespace:Bio.Views.Structure.ViewModels      5!        � 	Resources  5"         circleViewerContextMenu m� Command �� Path �� DataType<  6Bio.Views.Structure.ViewModels.CircleSequenceViewModel<  6Bio.Views.Structure.ViewModels.CircleTickMarkViewModel=  7Bio.Views.Structure.ViewModels.CircleTickLabelViewModel ��Name;  5Bio.Views.Structure.ViewModels.CircleElementViewModel F�Name � ToolTip �� Triggers �� Value	 �� Binding
 �� RelativeSource �� 
TargetName �� Property �� Value<  6Bio.Views.Structure.ViewModels.CircleBasePairViewModel �� OrientationE  ?Bio.Views.Structure.ViewModels.PhyloDInteractionCircleViewModel  phyloDInteractionsVS �� Source �� SortDescriptions+   %System.ComponentModel.SortDescription   PropertyName   	Direction��%  &	        (�� s     5&      *, +)(��     5-      *, +)(�� �    53      *, +)(�� �    5<      *, +)(�� 9    5R      *, +)(�� �    5m      *, +)&	 �    �� 5#   
   .@�5$      m� $��Load PhyloD Data��6    61   �� $ LoadPhyloDDataCommand�6b   5%      �� 5&   
   "  6   .R�5'      A� 
���   �$��3��5*      ��5'      �� $ Diameter���5(      �� $ Diameter�5+      5,      �� 5-   
   "  6   .R�5.      A� 
��������6   �� $
 Size���5/      �� $
 Size�51      52      �� 53   
   "  6   .R�54      �� $ 	TickLabel��6   ��55      �� $ TickText���56      �� $ Typeface.FontFamily���57      �� $ Typeface.Style���58      �� $ Typeface.Weight���59      �� $ 	LabelSize�5:      5;      �� 5<   
   "  6   .R�5=      F� $ arcElem��6   
������ $��2��5?      ��5@      �� $
 Path� 5A      �� 5B      ��6    �� $ Label�6>   5D       5E      �� 5F      $
 True��6t   	 6   �� 
 �� *TemplatedParent+$ IsMouseOver�.a�5G      �� $ arcElem��6/    � ��
 �  ��6R   �� 5H      $ arcElem��6-    � ��
 �  ��6P   5I      �� 5J      $
 True��6B   	 6   �� $ 
IsSelected�.a�5K      �� $ arcElem��6/    � ��
 � � �6T   5L      �� 5M      $ 1��6=   	 6   �� $ Index�.a�5N      �� $ arcElem��6/    � ��
 ��  �6S   5O      5Q      �� 5R   
   "  6   .R�5S      F� $ 
lineElemBP��6   
����+��$
��0.75��5U      ��5V      �� $ 	Connector� 5W      �� 5X      $ Vertical=�6!   .+�5Y      �� ��6$   �� $ BasePair�6E   �� 5Z      $ 
Horizontal=�6%   .+�5[      �� ��6(   �� $ FivePrimeIndex�6O   �� 5\      -6+   �� 5]      ��6(   �� $ ThreePrimeIndex�6P   5^      5_      5a       5b      �� 5c      $
 True��6B   	 6   �� $ 
IsSelected�.a�5d      �� $ 
lineElemBP��6/    � ��
 � � �6W   �� 5e      $ 
lineElemBP��68    � ��$	 1.5��6P   6^   5f      �� 5g      $
 True��6t   	 6   �� 
 �� *TemplatedParent+$ IsMouseOver�.a�5h      �� $ 
lineElemBP��6/    � ��
 ����6_   �� 5i      $ 
lineElemBP��68    � ��$	 1.5��6P   6^   5j      5l      �� 5m   
   "  6   .R�5n      F� $ lineElemPhyloD��6   
����� �$��1��5p      $y�Hidden=�5r      ��5q      �� $ 	Connector� 5s      �� 5t      $ Vertical=�6!   .+�5u      �� $ 
Horizontal=�6%   .+�5v      �� 	QValue:62   �� 5w      ��6(   �� $ QValue�6G   5x      �� 5y      $ 
Horizontal=�6%   .+�5z      �� 	PValue:61   �� 5{      ��6(   �� $ PValue�6G   5|      5}      5       5�      �� 5�      $
 True��6?   	 6   �� $ Visible�.a�5�      �� $ lineElemPhyloD��63    � y�$ Visible=�6O   6a   5�      �� 5�      $ False��6?   	 6   �� $ Visible�.a�5�      �� $ lineElemPhyloD��63    � y�$ Hidden=�6O   6`   5�      5�      �� 5�   
    5�      �� $ PhyloDVM.PhyloDElements� 5�        5�      $ PredictorIndex��6&   6F     5�      $ QValue��6&   $ 
Descending=�6<   6U   5�      5�      .��5�      ]�  � ContextMenu# [  6   .+�5�   
   �� $	��Top=�6   .@�5�      �� .��5�      �� $ 
Horizontal=�6   .+�5�      �� ��6    �� $ Status�6?   5�      5�      �� 5�      $��Right=�6   .��5�      �� $ 
Horizontal=�6   .+�5�      �� Show All Interactions67   �� 5�      -   5         ��Name$ visualInteractionFilterCB��5�      {�5�      �� $&  PhyloDVM.IsInteractionDataLoaded�6]   �� 5�      Filter Interactions65   �� 5�      -   5        $ interactionFilterCB��5�      {�5�      �� $&  PhyloDVM.IsInteractionDataLoaded� d� 	IsChecked 5�      �� $( "PhyloDVM.IsInteractionDataFiltered� �� Mode$ TwoWay=�6l   �� 5�      Zoom:6'   �� 5�      -   5         ��Name$ 
viewScaler��5�       � Value$ 1Q�61    � Minimum$ .5Q�6;    � Maximum$ 10Q�6H   $	��100��6U    �� TickFrequency$ .1Q�5�       �� TickPlacement$ TopLeft=�6,    �� Orientation$ 
Horizontal=�6D   6_   5�      5�      5�      � 5�   
    � RowDefinitions 5�      �� 5�      $��* �6    6-   �� 5�      $
��Auto �6    60   �� 5�      $
��0.5* �6    60   .+�5�      �� -   5         ��Name$ lb��5�      $��0��5�      ��5�      �� $ ModelElements���5�      Z� ��5�      �� 5�        �� 
TargetType"  ��6   .�5�      ��  �@��Top 67   �� $ Y�6R   �� 5�       �@��Left 68   �� $ X�6S   �� 5�       �@K�ZIndex 6:   �� $ Z�6U   �� 5�       �@��
IsSelected 67   �� $ 
IsSelected�$ TwoWay=�6h   �� 5�       � �� 5�      �� 5�   "   ! �� 
TargetType"! ��62   .R�5�   &   �� #��z��67   #��z��5�   3   5�   '   5�   #   5�      5�      ��5�      �� 5�      .R�5�      �� ��6!   �� $ Width���5�      �� $ Height�" � LayoutTransform" 5�      V� 5�   "   .~�5�   &   �� # �� ScaleX# 65   �� $ �� ElementName$$ 
viewScaler��$ Value�% �� ScaleY% 5�   %   �� $$ 
viewScaler��$ Value�& �� CenterX& 5�   %   �� 
 �� *FindAncestor+' �� AncestorType"' ��$ ContentHorizontalOffset�( �� CenterY( 5�   %   �� 
 �� *FindAncestor+"' ��$ ContentVerticalOffset�6�   5�   #   5�      5�      5�      �� 5�      $��1��6   
���   �$��2��5�      ) �� ResizeBehavior$) PreviousAndNext=�5�      $��Stretch=�5�      5�      �� 5�      -   5        * ��Name$* detailsView��5�      $��2��5�      ��5�      �� + �� Source#+ [ , �� View, 5�      �� 5�      .P�5�      �� $��Predictor Index��6)   - �� Width$
- Auto��5�   )   . �� CellTemplate. 5�      �� 5�   "   .R�5�   &   �� ��60   �� $ PredictorIndex�6W   5�   #   5�      �� 5�      $��Predictor Nt��6)   $
- Auto��5�   )   . 5�      �� 5�   "   .R�5�   &   �� ��60   �� $ PredictorIndexNucleotide�6a   5�   #   5�      �� 5�      $��Target Index��6)   $
- Auto��5�   )   . 5�      �� 5�   "   .R�5�   &   �� ��60   �� $ TargetIndex�6T   5�   #   5�      �� 5�      $��	Target Nt��6)   $
- Auto��5�   )   . 5�      �� 5�   "   .R�5�   &   �� ��60   �� $ TargetIndexNucleotide�6^   5   #   5     �� 5     $��PValue��6)   $
- Auto��5  )   . 5     �� 5  "   .R�5  &   �� ��60   �� $ PValue�6O   5  #   5
     �� 5     $��QValue��6)   $
- Auto��5  )   . 5     �� 5  "   .R�5  &   �� ��60   �� $ QValue�6O   5  #   5     5     5     5     5     5     