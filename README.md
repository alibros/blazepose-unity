# blazepose-unity
this project is based on [tf-lite-unity-sample](https://github.com/asus4/tf-lite-unity-sample)

In the sample scene you'll find a game object named `BlazePose`  that has the main script attached to it. 
The `Custom Camera Name` field is used to specify a camera name - leave this blank for it to pick up the default camera.

If you want to use the tablet you can tick `Use Front Facing Camera`  if you want to use the front facing camera.

The script will give you access to all body landmarks being tracked in real time using `worldJoins`  and to get you started I've already extracted left and right hand positions as game objects `leftHand`  and `rightHand` .


For debugging you can tick `DrawSitckFigure` for it to show the blue outline (as with the video I shared above) or enable the `Canvas`  game object to see the actual camera feed.
