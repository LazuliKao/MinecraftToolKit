   M S B A M L   `   `   `  ���� K  GMinecraftToolKit, Version=0.0.3.0, Culture=neutral, PublicKeyToken=null?$clr-namespace:MinecraftToolKit.PagesMinecraftToolKit.Pages  9!clr-namespace:Elements.ConvertersElements.Converters  &     MinecraftToolKit.Pages.Websocket   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     @d2http://schemas.microsoft.com/expression/blend/2008     6local$clr-namespace:MinecraftToolKit.Pages     V RMaterialDesignThemes.Wpf, Version=3.1.0.1244, Culture=neutral, PublicKeyToken=nullFmaterialDesign1http://materialdesigninxaml.net/winfx/xaml/themes  2
converters!clr-namespace:Elements.Converters    N� Title$  	websocket�� � 	Resources (   "Elements.Converters.SelectedToBool   SelectedToBool�� %   &	          .��� .+��  � RowDefinitions �� $��3* ��� $
��Auto ��� $��45 �.+��� -    ��Name$ OutPutRichTextBox��$��8,8,8,2q� �� 
LineHeight$ 1���� $��1��$��5q���Z� ��Z� .@��� $
��Left=���Z� ��Z� ��	� $��0q�$��	0,-3,0,-3q�$��Command��   MaterialDesignTabRadioButton#��[  d� 	IsChecked �� *Parent.IsSelected+ �� RelativeSource �� *Self+.���  � ColumnDefinitions �� $��* ��� $	��130 �.+��� -    ��Name$ SendText��$
��Auto=�	 �� TextWrapping$
	 Wrap=� ( $MaterialDesignFilledTextFieldTextBox#��[ 
 �� AcceptsReturn
 . )  #MaterialDesignThemes.Wpf.HintAssist   Hint$$ Enter your command text here !���� $��5,-2,3,0q�$��1��.+��� -    ��Name$ SelectedServer��$	��Top=�$��0,0,0,19q�$ Client��.  (MaterialDesignThemes.Wpf.TextFieldAssist   HasClearButton .  & "MaterialDesignFloatingHintComboBox#��[    
HelperText$ Select server to send��� $��Bottom=� �� $��60 ��� $��* �.+�� .+��� $��MutiCMD��$��Center=�d� -    d�Name$ MutiCMD��$��Bottom=��� -   $��1��{��� *SelectedIndex+ �� ElementName$ SelectedServer�� �� Mode$ OneWay=� �� 	Converter# [  .��'  !MaterialDesignThemes.Wpf.PackIcon     Kind$
 Send=��� ��Z� ��Z� ��	� $��0q�$��	0,-3,0,-3q�$��Message��#��[  �� *Parent.IsSelected+ �� *Self+.���  �� $��* ��� $	��130 �.+�� $��0,0,0,0q� �� $��26 ��� $��* �.+�� $��1�� �� ���� $ 
nickNameTX�� �� Path$ ActualWidth��� $��* �.+��� -    ��Name$ 
nickNameTX��$��16(�$��Center=�$
��Left=�$��Your nick name:���� -   $ 
nickNameTB��$��1��$��Server��$��5,0,10,0q��� -   $ SendMessageTB��$��0,1,10,0q�$
��Auto=�$
	 Wrap=�#��[ 
 . $ Input message here !��$��1��� $��1��.+��� -	   $ SelectedServerTab2��$	��Top=�$ Client�� . #��[ $ Select server to send��$��0,0,6,15q����� $ SelectedServer��$ Items� �� SelectedIndex �� $ SelectedServer��$ SelectedIndex��� -
   $��0,0,6,0q�$��Bottom=�{��� *SelectedIndex+$ SelectedServer��$ OneWay=�# [  .��  $
 Send=��� ��Z� ��Z� ��	� $��0q�$��	0,-3,0,-3q�$��Triggers��#��[  �� *Parent.IsSelected+ �� *Self+.���  �� $��40 ��� $��* �.+��� -   $ 
configFile��$��Center=� �� 
IsReadOnly . $��config file location:��$��0,0,70,0q��� -   $
��Edit��$��Right=��� ��Z� ��Z� ��	� $��0q�$��	0,-3,0,-3q�$��Advanced Build��#��[  �� *Parent.IsSelected+ �� *Self+.��� .+��  �� $��* ��� $��40 �.+��  �� $��* ��� $	��100 �.+��� $	��200��$
��Auto=�.���� -   $��MCFunctions��$
��Wrap=��� $��1��.+��� -   $ SelectedServerTab3��$��1��$	��Top=�$ Client�� . #��[ $ Select server to send��$��5,0,6,15q����� $ SelectedServer��$ Items� �� $ SelectedServer��$ SelectedIndex��� -   $	��5,0q�$��Bottom=�{��� *SelectedIndex+$ SelectedServer��$ OneWay=�# [  .��  $
 Send=�� $��1�� �� $��* ��� $��* ��� $��* �.+��� -   $
��Edit��$	��5,0q��� -   $
��Save��$	��5,0q�$��1���� -   $��Import��$	��5,0q�$��2��)  #MaterialDesignThemes.Wpf.DialogHost  -     Name$ 
EditDialog��$��0�� � Triggers :�  :� RoutedEvent$, &materialDesign:DialogHost.DialogOpened��.[��� .���� .��U�  �� TargetProperty$ (FrameworkElement.Height)�.\�9  3System.Windows.Media.Animation.EasingDoubleKeyFrame   O� KeyTime$ 0:0:0.3�� O� Value$	 300Q�   EasingFunction .  (System.Windows.Media.Animation.CubicEase  7  1System.Windows.Media.Animation.EasingFunctionBase   
EasingMode$ EaseIn=�:� $- 'materialDesign:DialogHost.DialogClosing��.[��� .���� .��U� $ (FrameworkElement.Height)�.\�  $ 0:0:0.3��$ 0Q�   $ EaseOut=�    DialogContent  �� $��10q�.+��� -   ! ��Name$! TabSelectMode��
���������Z� .@��� $��Square��.���� .+��� " �� Orientation$" 
Horizontal=�.+��� $��Start Point��$��Center=�$��80��$��15(��� $��x:��$��Center=��� -   $��	SquareSPx��$��Center=�$��20���� $��y:��$��Center=��� -   $��	SquareSPy��$��Center=�$��20���� $��z:��$��Center=��� -   $��	SquareSPz��$��Center=�$��20��� $��10���� -   $��SquarePlaceBlock1��$��Get Your Position��  MaterialDesignFlatButton#��[ �� $" 
Horizontal=�.+��� $��
 End Point��$��Center=�$��80��$��15(��� $��x:��$��Center=��� -   $��	SquareEPx��$��Center=�$��20���� $��y:��$��Center=��� -   $��	SquareEPy��$��Center=�$��20���� $��z:��$��Center=��� -   $��	SquareEPz��$��Center=�$��20��� $��10���� -   $��SquarePlaceBlock2��$��Get Your Position��#��[ �� $" 
Horizontal=�.+��� $��Block Name:��$��15(�$��Center=��� -   $��SquareBlockName��$��Center=�$��70���� $��5���� -   $��SquareFillMode�� # MaterialDesignToolToggleListBox#��[ $ 0��.@��� $
��Fill���� $
��Keep���� $��Destroy���� $��Hollow���� $��Outline���� $��Border��]� -   $��SquareSurfacePanel��$y�	Collapsed=�$��Center=�$��0,7,0,0q�.+��� $��Include Surface��$��Center=��� -    $��SquareIncludeSurface��$��8,0,0,0q�#��[ # �� SelectionMode$# Multiple=�.@��� $	��左��$ �� 
IsSelected$ . �� $	��上��$ . �� $	��前��$ . �� $	��右��$ . �� $	��后��$ . �� $	��下��$ . �� $��Round��.���� .+��� $" 
Horizontal=�.+��� $��Center Point��$��Center=�$	��100��$��15(��� $��x:��$��Center=��� -!   $��RoundCPx��$��Center=�$��20���� $��y:��$��Center=��� -"   $��RoundCPy��$��Center=�$��20���� $��z:��$��Center=��� -#   $��RoundCPz��$��Center=�$��20��� $��10���� -$   $��RoundPlaceBlock��$��Get Your Position��#��[ �� $" 
Horizontal=�.+��� -%   $��RoundCreateMode��$��Radius:��$��15(�$��Center=��� -&   $��RoundRadius��$��Center=�$��70���� % J� Opacity$% .5Q�$��+0.5=>��$��15(�$��Center=�d� -'   $��RoundModeToggle��$��Center=��� $" 
Horizontal=�.+��� $��Block Name:��$��15(�$��Center=��� -(   $��RoundBlockName��$��Center=�$��70���� �� -)   $��RoundFillMode��#��[ $ 0��.@��� $
��Fill���� $��Border��� $��5���� -*   $��RoundOrientation��#��[ $ 0��.@��� $	��xOz���� $	��xOy���� $	��yOz���� $��15���  �� $��* ��� $��20 ��� $��* �.+��� $��Cancel��$��0q�#��[ $2��,materialDesign:DialogHost.CloseDialogCommand���� 
�������$��1��$��1���� -+   $��Confirm��$��0q�#��[ $2��,materialDesign:DialogHost.CloseDialogCommand��$��2���� $��2��$" 
Horizontal=�$��
10,0,10,10q�.+�]� -,   & ]�Name$& StartButton��.+��� --    % !MaterialDesignFloatingHintTextBox#��[ $��0,-8,0,0q�$��19130��$��Center=� . .�� �� $" 
Horizontal=�$��-2,0,0,0q�.+�  $ AccessPoint=��� $
��Port��.���� -.   $��10,0,0,0q�$��Start��� $��10���� -/   ' ��Name$' 
StopButton��$
��Stop��{�.  � $��10���� -0   $ StateTextBlock��$��Center=�$��
Not opened��  -1   $ LoadingDialog��  �� $��22q�.+�� $��Center=�.+�  -2   (  Name$( loadingIconComplete��$��25��$��25��$ 	CheckBold=�� -3   ) �Name$) loadingProgressBar��$��25�� % !MaterialDesignCircularProgressBar#��[ * � Value$* 0Q�+ � IsIndeterminate+ . �� -4   $ 
LoadingTip��$��Loading��, �� TextAlignment$, Center=�