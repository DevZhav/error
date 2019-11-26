// =====================================================================
// Copyright 2018-2018 ToolBuddy
// All rights reserved
// 
// http://www.toolbuddy.net
// =====================================================================

Frame Rate Booster increases the frame rate of Unity based applications with zero effort from you.

USAGE
=====
  * Have your project use the Mono scripting backend. If you don't know what scripting backend your project uses, you can find that setting in Unity's "Player Settings" then "Other Settings"
  * Import the package
  * Rebuild your game
  * Your game is now optimized  

HOW DOES IT WORK
================
Frame Rate Booster implements some commonly used Unity operations in a more optimized way. When you build your application, Frame Rate Booster will make it use the optimized version of those operations rather than Unity's.

EXPECTED FRAME RATE GAIN
========================
It depends on how heavily your code relies on operations on vectors, quaternions and similar objects. The more such operations there are, the better the optimization will be.
* On benchmarks, I had a 10% increase.
* On my other asset, Curvy Splines, I got also a 10% increase for operations like mesh generation and splines cache building.
* On games doing thousands of geometry operations per frame (like moving a lot of objects), I expect a few percent increase at most. Not too much, but hey, it's free!
* On the remaining situations, I don't expect any noticeable increase.

LICENSE
=======
Asset is governed by the Asset Store EULA.

Contact & Support
=================
If you have any questions, feedback or requests, please write to admin@curvyeditor.com

VERSION HISTORY
===============
1.1.1
	Corrected the error message when multiple builds are detected inside the target folder
1.1.0
	Added warnings and helpful logs when trying to use Frame Rate Booster with unsupported platforms
1.0.0
	First release