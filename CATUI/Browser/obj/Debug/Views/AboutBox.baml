   M S B A M L   `   `   `  ����c  _System.Windows.Interactivity, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35qPclr-namespace:System.Windows.Interactivity;assembly=System.Windows.InteractivitySystem.Windows.Interactivity  5   @   [ WJulmar.Wpf.Behaviors, Version=1.0.0.5, Culture=neutral, PublicKeyToken=ff1c121d8ddc1581iHclr-namespace:JulMar.Windows.Interactivity;assembly=Julmar.Wpf.BehaviorsJulMar.Windows.Interactivity 6�    
BioBrowser   BioBrowser.Views.AboutBox   5   	   W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   jInteractivityPclr-namespace:System.Windows.Interactivity;assembly=System.Windows.Interactivity     6@   [julmarHclr-namespace:JulMar.Windows.Interactivity;assembly=Julmar.Wpf.Behaviors     6�     � Title$  AboutBox��5   	    � ShowInTaskbar .  $	��600��60   $	��256��6<    � WindowStartupLocation$ CenterScreen=�6I    � AllowsTransparency . 
������  � WindowStyle$
 None=�5   <   .��5      � -   5         � RowDefinitions 5   
   �� 5	      �� 5
      $
��Auto �6    � ColumnDefinitions 5   
   �� 5      $
��Auto �6   �� 5      $
��Auto �6   �� 5      $��* �6   �� 5      $��50 �6   .+�5   
   �� $��2��6   $��4��6!   $��Uniform=�66   $;��5/BioBrowser;component/images/splashScreen-600x256.png��6H   �� 5   
   $
��Left=�6   $��Bottom=�6,   $��1��6G   $��0��6T   $��Close��5      $
��10,5q�6!   $��	30,0,0,24q�60   .   (System.Windows.Interactivity.Interaction   	Behaviors 5      6  0JulMar.Windows.Interactivity.CloseDialogBehavior  5         DialogResult . 6-   5      �� 5   
   -   5        $��versionText��5   2   $��1��6   $��1��6"   $��Version 3.00��6E   ���	#FFBC683C$	��Top=�5      $��5q�6-   $
��Bold"�68   �� 5   
   -   5        	 ��Name$	 
grantsText��5      $��1��6)   $��2��66   ���	#F4A9A9B0$��with Grants from XY��6]   $��Right=�5       $	��Top=�61   $
��20,0q�5!      $��11(�6#   5"      5#      