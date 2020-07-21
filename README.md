# ReModuRutt

## 簡介 Introduction
ReModuRutt 為一 Visual Studio 2017 IDE 之程式碼轉換插件，主要以圖形化介面提供使用者程式碼轉換以及模組化製作轉換規則等功能。
- 執行平台 : Visual Studio 2017 
- 類別 : plug-in
- 撰寫語言 : C# 

ReModuRutt is a source code transformation tool plug-in on Visual Studio 2017 IDE, provides GUI to transform source code and create modular transformation rules. 
- Platform : Visual Studio 2017
- Type : plug-in
- Programming language : C#

---
## 安裝方式 How to install
ReModuRutt 可由 Visual Studio Marketplace 下載：
1. 於 Visual Studio 中點選`工具`，選擇`擴充功能和更新`
2. 點選`線上`並搜尋`ReModuRutt`
3. 點選安裝按鈕即可進行安裝

另外也能由[此連結](https://marketplace.visualstudio.com/items?itemName=ncupslab.ReModuRutt)下載安裝。

安裝後即可在Visual Studio IDE 中之工具選單使用此服務。\
![](https://i.imgur.com/FixSRoQ.png)

ReModuRutt can download from Visual Studio Marketplace:
1. In Visual Studio, on the Tools menu, click `Extensions and Updates`.
2. Click `Online` and then search for `ReModuRutt`.
3. Click Download. The extension is then scheduled for install.

This tool also can download from [this link](https://marketplace.visualstudio.com/items?itemName=ncupslab.ReModuRutt).

After complete the installation, user can find ReModuRutt in Visual Studio Tools menu.


## 如何使用 How To Use

主要功能可分成「規則製作 create rule」以及「程式碼轉換分析 Analysis and transferred」。
第一次使用時，需先選擇欲存放規則的資料夾(或是已存在的規則資料夾)。\
範例規則可[由此下載](https://github.com/ncu-psl/ReModuRutt/tree/master/AnalysisExtension/example)，並在選擇規則時選擇`Rule`資料夾。

The main tools can be divived into `create rule` and `Analysis and transferred`.\
When using for the first time, you need to select the folder where to store the rules, or select existing rule folder.\
Sample rule files can be downloaded [here](https://github.com/ncu-psl/ReModuRutt/tree/master/AnalysisExtension/example), and choose folder `Rule` when select existing rule folder.


### 規則製作 create rule
step 1. 於工具列中點選`create rule`\
step 2. 選擇已存在的規則資料夾，或是欲存放規則的資料夾 (若已選擇過則會跳過此步驟)\
step 3. 點選左側列表中的規則進行編輯
- 編輯方式：`before`為轉換前規則，`after`為轉換後規則
    - 新增文字：可直接輸入，或是於已開啟專案中選擇文字，點選右鍵後加入
    ![](https://i.imgur.com/bh7Fx9m.png)
    - 區塊設置：選擇某段文字右鍵設為區塊，或直接右鍵加入區塊
    ![](https://i.imgur.com/74HfXbj.png)
    - 清除內容：點選`clear`按鈕可清除該製作區域內容
    - 複製內容：點選`copy from before`可直接複製轉換前規則至轉換後規則製作區塊    

step 1. Click `create rule` in the Tools menu \
step 2. Select the folder where to store the rules, or select existing rule folder.(If  already selected, skip this step)\
step 3. Click the rule in the list on the left to edit
- How to edit : `before` is the rule edit block that before transferred, `after`is the rule edit block that after transferred.
    - Add text : user can enter text directly, or select text in an already opened file, right-click to add it into rule edit block.
    - Set block : right-click to select a piece of text as a block, or right-click to add a block.
    - Clear : click the `clear` button to clear the content of rule edit block.
    - Copy : click the `copy from before` button to directly copy the content in `before` rule edit block to the `after` rule edit block. 

### 程式碼轉換分析 Analysis and transferred
step 1. 開啟專案或檔案\
step 2. 於工具列中點選 `Analysis and transferred`，選擇要分析的檔案或專案並點選下一步\
step 3. 選擇欲進行轉換的規則集合，並點選下一步\
step 4. 確認轉換與分析之資訊\
step 5. 確認轉換後結果\

step 1. Open project or file\
step 2. Click `Analysis and transferred` in the Tools menu, select the file to be analysis and click `Next`.\
step 3. Select the rule set that used to analysis and click `Next`\
step 4. Check infomation.\
step 5. Check final result.\

---

## 編輯並重新發佈插件 How to edit plug-in and republish
若欲針對此插件進行修改並重新發佈，詳細發佈方法可參照[此文件](https://github.com/ncu-psl/wiki/blob/master/Deployment/Visual%20Studio%20Marcketplace%20plug-in%20publish%20direction.md)，或[微軟官方說明文件](https://docs.microsoft.com/en-us/visualstudio/extensibility/walkthrough-publishing-a-visual-studio-extension?view=vs-2019)。

If need to edit this plug-in and republish，publish step can refer to [this document](https://github.com/ncu-psl/wiki/blob/master/Deployment/Visual%20Studio%20Marcketplace%20plug-in%20publish%20direction.md), or [Microsoft Visual Studio  document](https://docs.microsoft.com/en-us/visualstudio/extensibility/walkthrough-publishing-a-visual-studio-extension?view=vs-2019)

---

## 如何轉移至其他平台 How to transfer to other platforms

若要將 ReModuRutt 轉移至其他平台實作，你可以：

1. 將 plug-in 讀取檔案部分去除掉，並且：\
    solution 1. 用其他方式取得檔案列表，並接在`View\ChooseFileWindowControl.xaml.cs`上顯示。
            檔案列表儲存形式請參照`Model\FileTreeNode.cs`            
    solution 2. 用其他方式讓使用者選擇欲分析的檔案，並將選擇的檔案傳至`View\ChooseAnalysisWindowControl.xaml.cs`繼續後續動作    
2. 將 plug-in 右鍵取得目前 project 選取文字部分去除掉，用其他方式取得選取文字，並接在`Tool\StaticValue.cs`的method`public static void AddTextIntoRuleCreateFrame(string selectContent)`之上。

If need to transfer ReModuRutt to other platforms, you can:
1. Remove read file in plug-in, and :
    solution 1. Use other method to get file list,then show the file list in `View\ChooseFileWindowControl.xaml.cs`.
            File list form can see `Model\FileTreeNode.cs`.            
    solution 2. Use other methods to get file that user want to analysis, and transfer the selection file to `View\ChooseAnalysisWindowControl.xaml.cs` to continue. 
2. Remove method that right-click get selection text in project which open now, use other methods to get selection text, and transfer it to method`public static void AddTextIntoRuleCreateFrame(string selectContent)` in `Tool\StaticValue.cs`.
---
### Plug-in class
程式與 IDE 銜接的部分為`PlugInModel`內的 class 以及`ToolListPackage.vsct`。
- PlugInModel
    - ToolListPackage.cs
        - 註冊使用 visual studio plug-in
    - AnalysisToolListCommand.cs
        - 顯示分析轉換功能於工具選單中
    - CreateRuleToolWindowCommand.cs
        - 顯示規則製作功能於工具選單中
    - RightClickAddRuleWindowCommand.cs
        - 取得目前 project 所選取文字，並加入規則製作頁面
    - PlugInTool.cs
        - 用來存取 plug-in 資訊並回傳給程式中的其他模組，目前主要是用來取得 **Project 列表資訊**
- ToolListPackage.vsct
    - plug-in 設定檔，可由此設定在工具列選單中的按鈕文字、圖式等資訊

`ToolListPackage.vsct` and class that in package `PlugInModel` are used to connect program and IDE.
- PlugInModel
    - ToolListPackage.cs
        - Register visual studio plug-in
    - AnalysisToolListCommand.cs
        - Display analysis and transform services in Tools menu
    - CreateRuleToolWindowCommand.cs
        - Display rule creator services in Tools menu
    - RightClickAddRuleWindowCommand.cs
        - get selection text in project which open now, add selection text into rule creation frame
    - PlugInTool.cs
        - Used to get plug-in information and return it to other modules in program, currently mainly used to get **file list  information of project which is open now**. 
- ToolListPackage.vsct
    - plug-in setting file, can set button text, picture or other infomation in Tools menu by this setting file.
