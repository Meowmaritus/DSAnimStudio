# Getting Started: Visual Studio 2017 (with example project)
* Download and run the .msi installer for IronPython and follow the directions (make sure Visual Studio is closed): https://github.com/IronLanguages/main/releases/tag/ipy-2.7.7
* Install Python support in Visual Studio 2017:
  * Run the Visual Studio Installer (Quick way to get to it is under "Programs and Features" right click on Microsoft Visual Studio 2017 and click "Change"
  * Click "Modify"
  * Switch to the "Individual Components" tab
  * Check the box next to "Python language support"
    * Note: You do not need to install Python 2 or 3 runtime since you'll be using the IronPython runtime instead 
* Download the **source code** for the latest pre-release of DarkSoulsScripting: https://github.com/Wulf2k/DarkSoulsScripting/releases
* Open the solution in Visual Studio 2017
* Build the solution (*F6-key*).
* Right click the *ScriptTester* project and select "Unload project", then right click on it again and select "Reload project".
* Double-click the `ScriptTester.py` script in the *ScriptTester* project.
* Make sure the latest Steam version of Dark Souls: Prepare to Die Edition is running.
* In Dark Souls, load a character of your choosing. 
* Press Alt+Shift+F5 to execute the test script in the Python interactive console window and watch it do a lot of weird stuff to your game. You can follow along to help you understand how the scripting works in general.
  
# Getting Started: PyCharm
* Download and run the .msi installer for IronPython and follow the directions (make sure Visual Studio is closed): https://github.com/IronLanguages/main/releases/tag/ipy-2.7.7
* Download and install the latest version of PyCharm (the free Community version works fine): https://www.jetbrains.com/pycharm/download/
* Create a new project in PyCharm:
  * For the interpreter, select your IronPython installation (it should be listed for you)
* Download the **binary / .DLL** for the latest pre-release of DarkSoulsScripting: https://github.com/Wulf2k/DarkSoulsScripting/releases
* Copy and paste the `DarkSoulsScripting.dll` file you just downloaded directly into your PyCharm project directory
* Add a new python file to your project (Alt+Insert or File->New... to open the add file popup menu)
* Import IronPython and DarkSoulsScripting namespaces:
```python
import clr
clr.AddReference("DarkSoulsScripting")
from DarkSoulsScripting import *
from DarkSoulsScripting.Extra import *
from DarkSoulsScripting.AI_DEFINE import *
from DarkSoulsScripting import IngameFuncs as f
from DarkSoulsScripting import ExtraFuncs as ex
```
* **Important**: Place your text cursor on `DarkSoulsScripting`. When hovering your mouse over the text, PyCharm will give you an error:

![Unresolved reference 'DarkSoulsScripting'](https://i.imgur.com/UXKe8kR.png)

  * To resolve this error, you must open the quick action menu (Alt+Enter) and choose the option to generate stubs for the DarkSoulsScripting module:
  
  ![Generate stubs for binary module DarkSoulsScripting](https://i.imgur.com/yLDioDE.png)

  
# Things to note:
* Saving is automatically disabled for the rest of the play session each time you run **ANY** script. This will allow you to test things without overwriting your save (you can manually call 'SetSaveEnable' in the `Functions.Extra` module to re-enable it if you choose)
