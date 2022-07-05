using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class SlideAnimationButton : MonoBehaviour {

    //animator reference
    private Animator anim;


    void Start () {
        //get the animator component
        anim = GetComponent<Animator>();
        //disable it on start to stop it from playing the default animation
        anim.enabled = false;
    }

    public void playAnimation(){

        //enable the animator component
        anim.enabled = true;
        //play the slide animation
        anim.Play("ButtonSlideAnimation");
    }

    public void delayedAnimation(float timeToWait){
        Invoke("playAnimation", timeToWait);
    }
    public bool AnimatorIsPlaying(){
         return anim.GetCurrentAnimatorStateInfo(0).length >
                anim.GetCurrentAnimatorStateInfo(0).normalizedTime;
     }

    void Update() {
        if (!AnimatorIsPlaying()){//Once animation is done => destroy the gameObject
            GameObject.Destroy(gameObject);
        }
    }

}