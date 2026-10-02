using System;
using System.Collections.Generic;
using MyUtils.TalkUtils;
using UnityEngine;

namespace _Projects.Features.Unit
{
    public class UnitTalk : MonoBehaviour
    {
        private void Start()
        {
            TalkManager.Instance.TalkAsync(new List<LineData>
            {
                new LineData("Hello, this is a test line 1.", ""),
                new LineData("This is line 2 of the conversation."),
                new LineData("And this is the final line, line 3.")
            });
        }
    }
}