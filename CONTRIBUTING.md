## NOTES FOR CONTRIBUTORS:

### 1. Major disclaimer:
I believe it is important for anybody willing to work on this game that they do so knowing that their contribution may directly be included in paid copies of this game.
However, as I hope is made very clear, this game is still free and open source. By contributing your code/assets you agree to it being placed under the copyleft GPL-3.0 license (you can read more on it here: https://www.gnu.org/licenses/quick-guide-gplv3.html
If you wish, you may credit yourself in the Contributors JSON file in ``GameData/Misc/Contributors.json``. This file is displayed ingame.

### 2. Conventions and "rules" for contributing:
* You will need to build the Godot editor from source with double precision enabled - see instructions in BuildingGodotEditor.md, and ask on Discord if you need help.
* Use the version of Godot specified in README.
* When starting a pull request, you MUST state what you changed/added down to each file.
* This is more or less just a general "keep your code clean" rule, but please ensure that any major changes you make don't break any other existing system.
* Following up with the previous rule, make sure anything you add is thoroughly tested.
* Avoid modifying work-in-progress systems. This can easily cause a conflict if said original developer of the system intended it to function one specific way.
* Keep your commits small. I don't like reading through hundreds of lines of code.

### 3. Help me review pull requests!
I (Sushut) am fairly busy a lot of the time. I greatly appreciate it if the community could check pull requests for anything ranging from potential malicious code to simple conflicts. Strength in numbers and all.
