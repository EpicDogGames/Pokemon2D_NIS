using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteAnimator
{
    SpriteRenderer spriteRenderer;
    List<Sprite> frames;
    float frameRate;

    int currentFrame;
    float timer;

    // 60 frames per second
    public SpriteAnimator(List<Sprite> frames, SpriteRenderer spriteRenderer, float frameRate=0.16f)
    {
        this.frames = frames;
        this.spriteRenderer = spriteRenderer;
        this.frameRate = frameRate;
    }

    public void Start() 
    {
        currentFrame = 0;
        timer = 0f;
        spriteRenderer.sprite = frames[0];    
    }

    public void HandleUpdate()
    {
        timer += Time.deltaTime;
        if (timer > frameRate)
        {
            // mod functions as if current frame is last frame resets to first frame
            currentFrame = (currentFrame + 1) % frames.Count;
            spriteRenderer.sprite = frames[currentFrame];
            timer -= frameRate;
        }
    }

    public List<Sprite> Frames 
    {
        get { return frames; }
    }
}
