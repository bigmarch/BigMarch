----------------------------------------------------------------------------------------------------
介绍

一个完整的BigMarch BlendTree应该由四部分构成，从细节到大局，分别是：
1，	StateNode，状态节点，一个"永远不停歇的"状态，当高层向它要数据时，返回指定类型的数据。
	StateNode不能有子节点。
	StateNode需要使用者自己定制，继承Node节点，实现GetResult方法。

2，	AddNode或者BlendNode，用于组织各个节点。从子节点得到数据之后，前者将它们想加，后者将它们混合。
	AddNode和BlendNode不能是子节点。
	AddNode和BlendNode必须使用BigMarch的，不需要使用者实现。

3，	BlendTree，树根。提供一个AutoSetup按钮，用于初始化。
	BlendTree必须使用BigMarch的，不需要使用者实现。

4，	使用BlendTree的客户，（粒子中的CamerRig类）。通过BlendTree设置各个BlendNode的Weight，或者向各个StateNode发消息。
	需要使用者自己实现。

5，	BlendData，各个节点之间传递的数据类型。
	需要使用者自己实现，继承BlendData类。



----------------------------------------------------------------------------------------------------
BlendTreeExample_CameraRig查看指南

1、	将五个普通状态下的camera position进行混合，五个位置分别代表五种状态下的camera位置。，“前进”，“静止”，“后退”，“氮气瞬间加速”，“氮气持续加速”。
	这个5个StateNode分别是： StateNode_Back，StateNode_Idle，StateNode_Forward，StateNode_N2oStart，StateNode_N2oStay。
	他们的父节点是：BlendNode_NormalPositions（表示normal状态下的位置。）

2、	将普通状态下的camera position和shake节点进行混合。
	shake节点分别是：StateNode_NormalHurtShake，StateNode_NormalHitShake。

3、	将普通状态和瞄准镜状态进行混合。
	分别是：AddNode_Normal，AddNode_Scope。
	父节点是：BlendNode_Fix（表示fix状态下的位置）

4、	玩家死亡后，会切换到自由旋转围观状态，该状态会接收输入，移动相机。
	混合节点分别是：BlendNode_Fix，StateNode_FreeRotation
	父节点是：BlendNode_Root
