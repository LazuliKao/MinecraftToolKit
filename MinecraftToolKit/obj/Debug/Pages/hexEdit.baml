   M S B A M L   `   `   `  ����K  GMinecraftToolKit, Version=0.0.3.0, Culture=neutral, PublicKeyToken=null?$clr-namespace:MinecraftToolKit.PagesMinecraftToolKit.Pages  5      $    MinecraftToolKit.Pages.HexEdit   5      R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5      R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      6local$clr-namespace:MinecraftToolKit.Pages     5      V RMaterialDesignThemes.Wpf, Version=3.1.0.1244, Culture=neutral, PublicKeyToken=nullFmaterialDesign1http://materialdesigninxaml.net/winfx/xaml/themes  5        N� Title$  hexEdit��5      .��5      �  � RowDefinitions 5   
   �� 5      $��48 �6   �� 5      $��2* �6   �� 5      $��50 �6   �� 5      $��3* �6   .+�5   
   ��  �� Orientation$ 
Horizontal=�6   $��0q�6.   .+�5      o� $��0q�6   ��6   Z� .@�5      m� $
��File��6    m� Icon 5      '  !MaterialDesignThemes.Wpf.PackIcon  5         Kind$
 File=�62   .@�5      m� -   5        $��	Open FIle��5      
������� 5        5      $
 File=�66   5      m� 5       -   5        $��	Save FIle��5       
������� 5!        5"      $ ContentSave=�66   5$      5%      50      �� 51      -   5         ��Name$ SafeModeToggleButton��51      $��ReplaceMode��65      MaterialDesignCheckBox#��[  6L   52      �� 53   
   -   5         ��Name$ 
TextboxHex��53       ( $MaterialDesignFilledTextFieldTextBox#��[ 6+   )  #MaterialDesignThemes.Wpf.HintAssist   Hint$ Hex Text Here��6q    �� AcceptsReturn . "	 �� VerticalScrollBarVisibility$
	 Auto=�54   '   $
��Auto��6J   $��10q�6X   $��1��6d   
 �� TextWrapping$

 Wrap=�6q   6�   � 55   
   $��2��6   $��	10,0,10,0q�6   y�6/   ��  �� ElementName$ SafeModeToggleButton�� �� Path$ 	IsChecked� �� Mode$ OneWay=�   BooleanToVisibilityConverter �� 	Converter# [  � ColumnDefinitions 56      �� 57      $��60 �6#   �� 58      $��50 �6$   �� 59      $��* �6$   �� 5:      $��95 �6#   .+�5<      �� $��Center=�6   $��Center=�67   .+�5=      �� $	��Hex��6   $��Center=�6'   d� 5>      -   5         d�Name$ HexReplaceToggleButton��5>        $  MaterialDesignSwitchToggleButton#��[ 6@    d� 	IsChecked$
 TrueY�6|   6�   5?      � 5@      $��1��6   .+�5A      �� $��Before��6   $��15(�6*   $��0,5,0,0q�68   �� 5B      $��After��6   $��15(�6)   $��0,28,0,0q�67   5C      �� 5D      $��2��6   $��0,0,8,0q�6*   $��Center=�6;   .+�5E      �� -   5        $ replaceBefore��5E      �� 5F      -   5        $ replaceAfter��5F      5G      �� 5H      -   5        $��3��5H      $��0q�6%   .��5I      �� $ 
Horizontal=�6   .+�5J        $��Center=�6.   $��Center=�6K   $ FindReplace=�6g   $��30��6z   $��30��6�   �� 5K      $��Replace��6    $��Center=�6/   5L      5M      5N      � 5O   
   $��2��6   $��	10,0,10,0q�6   y�6/   �� $ SafeModeToggleButton��$ 	IsChecked�$ OneWay=�  InverseBoolToVisConverter# [  5P      �� 5Q      $��1* �6#   �� 5R      $��10 �6#   �� 5S      $��1* �6#   .+�5U      �� -	   5        $��0q�5U      .��5V      �� $ 
Horizontal=�6   .+�5W      �� $��Convert to Hex��6    $��Center=�66   � 5X      .+�5Y        $��Center=�62   $��Center=�6O   $ ArrowTopBold=�6j   $��17��6~   $��17��6�     5Z      $��Center=�62   $��Center=�6O   $
 Sync=�6k   $��40��6w   $��40��6�   5[      5\      5]      �� 5^      -
   5        $��3��5^      $��0q�6%   .��5_      �� $ 
Horizontal=�6   .+�5`      � .+�5a        $��Center=�62   $��Center=�6O   $ ArrowBottomBold=�6j   $��17��6�   $��17��6�     5b      $��Center=�62   $��Center=�6O   $
 Sync=�6k   $��40��6w   $��40��6�   5c      �� 5d      $��Convert to Utf-8��6    $��Center=�68   5e      5f      5g      �� 5h   
   -   5        $ TextboxUtf8��5h      #��[ 6(   $ Parsed UTF-8 decoded text��6n    . $
	 Auto=�5i   u   $
��Auto��6�   $��10q�6�   $��3��6�   $

 Wrap=�6�    �� 
IsReadOnly 6   �� $ SafeModeToggleButton��$ 	IsChecked�$ OneWay=�.��6�   5j      5n      5o      