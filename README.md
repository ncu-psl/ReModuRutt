# ReModuRutt

## 簡介
這是一個用於分析與轉換的工具，可以依照規則樣式自訂欲轉換或分析的規則。\
此程式將分析轉換功能做成 visual studio 2017 的 plug in，使用者可以藉由 工具列->工具->( plug in 名稱 ) 來使用此功能。

- 執行平台 : visual studio 2017 
- 類別 : plug in
- 撰寫語言 : C#

## 安裝方式
此專案可由 Visual Studio Marketplace 下載， 可由IDE->工具->擴充功能和更新->線上->搜尋ReModuRutt ，或是由[此連結](https://marketplace.visualstudio.com/items?itemName=nori.ReModuRutt)下載安裝，安裝後即可在Visual Studio IDE 中之工具選項使用此服務。

![](https://i.imgur.com/FixSRoQ.png)

## 程式架構
### Plug in
將程式與IDE銜接的部分為`PlugInModel`內的class以及`ToolListPackage.vsct`兩部分。

- PlugInModel
    - ToolListPackage.cs
        - 註冊使用visual studio plug in
    - ToolListCommand.cs
        - 執行 plug in 被點選後的動作，點選後執行`Execute(object sender, EventArgs e)`
    - PlugInTool.cs
        - 自行撰寫的模組，用來存取 plug in 資訊並回傳給程式中的其他模組，目前主要是用來取得**Project列表資訊**
- ToolListPackage.vsct
    - plug in 設定檔，可由此設定在工具列選單中的按鈕文字、圖式等資訊

- 如何轉移\
若要將此功能以其他方式來實作，你可以將 Plug in 部分的檔案去除掉，並且：\
solution 1. 用其他方式取得檔案列表，並接在`View\ChooseFileWindowControl.xaml.cs`上顯示。\
            檔案列表儲存形式請參照`Model\FileTreeNode.cs`\
solution 2. 用其他方式讓使用者選擇欲分析的檔案，並將選擇的檔案傳至`View\ChooseAnalysisWindowControl.xaml.cs`繼續後續動作
