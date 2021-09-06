/******************************************************************************
 * Copyright (C) Ultraleap, Inc. 2011-2020.                                   *
 * Ultraleap proprietary and confidential.                                    *
 *                                                                            *
 * Use subject to the terms of the Leap Motion SDK Agreement available at     *
 * https://developer.leapmotion.com/sdk_agreement, or another agreement       *
 * between Ultraleap and you, your company or other organization.             *
 ******************************************************************************/

using UnityEngine;
using System.Collections;
using System;
using Leap;
using UnityEngine.Events;

namespace Leap.Unity
{
    public class HandEnableDisable : HandTransitionBehavior
    {
        public UnityEvent OnHandReset;
        public UnityEvent OnHandFinish;
               

        protected override void Awake()
        {
            base.Awake();
            gameObject.SetActive(false);
            
        }


        protected override void HandReset()
        {
            OnHandReset.Invoke();
            gameObject.SetActive(true);
        }

        protected override void HandFinish()
        {
            OnHandFinish.Invoke();
            gameObject.SetActive(false);
        }
    }
}
