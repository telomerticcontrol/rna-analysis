   M S B A M L   `   `   `  ����N  JBio.Views.Structure, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullO,clr-namespace:Bio.Views.Structure.ViewModelsBio.Views.Structure.ViewModels  5       9!clr-namespace:Bio.Views.StructureBio.Views.Structure  5!      3    -Bio.Views.Structure.SecondaryStructureBrowser   5      R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5      R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      C
ViewModels,clr-namespace:Bio.Views.Structure.ViewModels     5       7	Structure!clr-namespace:Bio.Views.Structure     5!        � 	Resources  5"      6   0Bio.Views.Structure.StructureModelColorConverter   ColorConverter �� DataType6   0Bio.Views.Structure.ViewModels.SSSymbolViewModel ��Name �� Path �� Triggers �� Value �� Binding �� 
TargetName �� Property	 �� Value8   2Bio.Views.Structure.ViewModels.SSBasePairViewModelG   ABio.Views.Structure.ViewModels.SSBasePairCircleConnectorViewModel
 A�Name �� 	ConverterE   ?Bio.Views.Structure.ViewModels.SSBasePairLineConnectorViewModel ��Name	 �� X1	 �� Y1 J� RenderTransformOrigin	 �� X2	 �� Y29   3Bio.Views.Structure.ViewModels.SSTextLabelViewModel9   3Bio.Views.Structure.ViewModels.SSLineLabelViewModelB   <Bio.Views.Structure.ViewModels.SSParallelogramLabelViewModel � Points8	   2Bio.Views.Structure.ViewModels.SSArcLabelViewModel:
   4Bio.Views.Structure.ViewModels.SSArrowLabelViewModel � Points �� Angle��%�
  &	        (��      5%      *, +)(�� �    54      *, +)(�� �    58      *, +)7   7   (�� V    5L      *, +)7   (��     5]      *, +)(�� �    5f      *, +)(�� �    5p      *, +)(�� k    5w      *,	 +)(�� �    5}      *,
 +)  5#   
   6J   �� 5%   
   "  6   .R�5&      �� $ NTSymbol��6   ��5'      �� $ Symbol.Value���5(      �� $ Typeface.FontFamily���5)      �� $ Typeface.Style���5*      �� $ Typeface.Weight���5+      �� $ FontSize�5,       5-      �� 5.      $ False��6?    6   �� $ Visible�.a�5/      �� $ NTSymbol��63    � y�$	 	Collapsed=�6I   6]   50      52      �� 54   
   "  6   56      �� 58   
   "  6   .R�59      � ��6   �� $ BasePair.Width���5:      �� $ BasePair.Height�.+�5;      � ��6   �� $( "BasePair.FivePrimeNucleotide.Width���5<      �� $) #BasePair.FivePrimeNucleotide.Height�.+�5=      A� $
 BPConnectorCircle��6   ��5>      �� $ Color�8   ��5?      �� $ 	Thickness���5@      �� $ Diameter���5A      �� $ Diameter�5B      5C      5D       5E      �� 5F      $
 True��6>    6   �� $ Filled�.a�5G      �� $ BPConnectorCircle��6-   	 6L   �� $ Color�8   � ��6�   5H      5J      �� 5L   
   "  6   .R�5M      �� $ BPConnectorLine��6   $ 0��5N      $ 0��5O      $ .5,.5&�5T       5P      �� $ Width� 5Q      �� $ Height���5R      �� $ Color�8   ��5S   
   �� $ 	Thickness�5U       5V      �� 5W      $ False��6?    6   �� $ Visible�.a�5X      �� $ BPConnectorLine��63    � y�$	 	Collapsed=�6P   6d   5Y      5[      �� 5]   
   "  6   .R�5^      �� ��6   �� $
 Text���5_      �� $ 
FontFamily���5`      �� $ FontSize���5a      �� $ Color���5b      �� $ 
FontWeight���5c      �� $ 	FontStyle�6?   5d      �� 5f   
   "  6   .R�5g      �� $	 0.0��6   $	 0.0��5h       5i      �� $ X1� 5j      �� $ Y1���5k      �� $ Color���5l      �� $ 	Thickness�5m      5n      �� 5p   
   "  6   .R�5q      �  6   �� $ VertexPoints���5r      �� $ Color���5s      �� $ 
LineWeight�5t      5u      �� 5w   
   " 	 6   .R�5x      F� ��6   �� $ Color���5y      �� $ 
LineWeight���5z      �� $ Curve�61   5{      �� 5}   
   " 
 6   .R�5~      �  6   �� $ 
EdgePoints���5      �� $ Color���5�      �� $ 
LineWeight�z�5�      �� 5�       6&   �� $ RotationAngle�6M   5�      5�      5�      .��5�      ]� .+�5�   
   �� $	��Top=�6   .@�5�      �� .��5�      �� ��6   ��  �� ElementName$ ss��$ SelectedItem�6Q   5�      �� 5�      $��Right=�6   .��5�      �� -   5         ��Name$ 
viewScaler��5�       � Value$ 1Q�5�       � Minimum$ .5Q�5�       � Maximum$ 10Q�5�      $	��100��5�       �� TickFrequency$ .5Q�5�      6.   5�      5�      �� 5�   
   -   5         ��Name$ ss��5�      ��5�      �� $ ModelElements���5�      Z� ��5�      �� 5�       �� 
TargetType" ��6   .�5�      ��  �@��Left	 64   �� $ X�6O   �� 5�       �@��Top	 63   �� $ Y�6N   �� 5�       � ��	 5�      �� 5�       �� 
TargetType" ��6.   .R�5�   "   ��  ��Name$ ssItemBd��6)   $��1q�5�   )   $ .5,.5&�5�   )   ._�5�   &   �� #��z��67   #��z��5�   7   5�   '   5�   #    �� Triggers 5�   "   P� 5�   &     P� Property  �@��
IsSelected! P� Value! . . �5�   *   �� $ ssItemBd��6H    � ��
	 � ���6^   5�   '   5�      5�      5�      ��5�      �� 5�      .R�5�      �� ��6   �� $ MaxWidth���5�      �� $ 	MaxHeight�" � LayoutTransform" 5�      V� 5�      .~�5�   "   �� # �� ScaleX# 61   �� $ 
viewScaler��$ Value�$ �� ScaleY$ 5�   1   �� $ 
viewScaler��$ Value�% �� CenterX% 5�   1   �� & �� RelativeSource& �� *FindAncestor+' �� AncestorType"' ��$ ContentHorizontalOffset�( �� CenterY( 5�   1   �� & �� *FindAncestor+"' ��$ ContentVerticalOffset�6�   5�      5�      5�      5�      5�      5�      