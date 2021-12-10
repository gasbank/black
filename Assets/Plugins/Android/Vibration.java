package com.pronetizen.sushi;

import android.os.Vibrator;
import com.unity3d.player.UnityPlayer;
import com.unity3d.player.UnityPlayerActivity;

class UnityVibration{


    public void Vibration(long length){
        Vibrator vibrator =(Vibrator)UnityPlayer.currentActivity.getSystemService(UnityPlayerActivity.VIBRATOR_SERVICE);
        vibrator.vibrate(length);
    }
}
