using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IDH.MyRoom;

class VisitMyRoomUIController : MyRoomUIController
{
    protected override void Awake()
    {
        // do nothing
    }

    public override void Initialize(MyRoomSystemRefParameter parameter)
    {
        goCleaningAllButton.SetActive(false);
        InitParams(parameter);
    }

    protected override void Update()
    {
        // do nothing
    }

    protected override void OnDestroy()
    {
        // do nothing
    }
}
