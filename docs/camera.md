# "Souls like" 镜头控制
在魂系列游戏中,镜头控制的策略主要有两种不同情况:一是无锁定,玩家可自由旋转.二是锁定目标,镜头的角度和位置取决于玩家和锁定目标的位置.
另外还有一点就是镜头碰撞.

## 无锁定镜头
首先看GIF,效果如下.

![](https://raw.githubusercontent.com/knightlyj/demo/master/docs/img/ds-nolock.gif)

可以看出来,有以下几个特点:

&emsp;&emsp;1.角色并没有一直处于屏幕中心.

&emsp;&emsp;2.朝着一个方向跑时,镜头不会偏离太多,且偏离程度稳定.

&emsp;&emsp;3.当角色停下来时,随着镜头接近预期位置,镜头运动速度会逐渐降低.


由此推测,镜头追踪速度取决于镜头与角色位置的差异.当角色偏离镜头中心较远时,镜头运动速度与角色速度相同.当角色偏离中心较近时,镜头运动速度也较慢.
这样一来,只要设置适当的参数,角色会稳定在镜头中心的一定范围内,是典型的负反馈模型.


另外看下面只有横向移动的效果.

![](https://raw.githubusercontent.com/knightlyj/demo/master/docs/img/ds-nolockHor.gif)

可以看出来,横向移动时,镜头位置几乎没有移动,只有角度改变,可以推测出镜头位置和角度算法.
已知镜头和角色当前位置,不改变镜头到角色的角度,根据预设的镜头距离(d),计算出镜头期望到达的位置.图示如下:

![](https://raw.githubusercontent.com/knightlyj/demo/master/docs/img/camera-free.gif)

根据以上分析,可以这样来实现功能,首先根据目前状态,计算出镜头需要到达的目标位置和角度,再向这个目标平滑运动即可.

这个算法分成两个部分实现:
	1.让一个点平滑追踪角色位置
	2.镜头看向这个点,直接设置期望的位置,不用做平滑处理.

实现出来的代码大致如下:
```
Vector3 watchPoint; //镜头看向的点
void Update(){
	//watchPoint平滑追踪角色
	Vector3 toPlayer = sight.position - watchPoint;
    float step = Mathf.Max(toPlayer.magnitude * scale, minStep); //scale用来控制追踪速度,minStep为最小追踪步长.
    watchPoint = Vector3.MoveTowards(watchPoint, sight.position, step);

	//镜头位置和角度设置
	float yaw = Vector3.AngleBetween(Vector3.forward, toWatchPoint);
	Quaternion rotation = Quaternion.Euler(0, yaw, 0);
	transform.rotation = rotation;

	Vector3 watchDir = rotation * Vector3.forward;
	transform.position = watchPoint - watchDir * cameraDistance; //cameraDistance为预设的镜头距离
}
```

赶紧运行看看,效果如下

![](https://raw.githubusercontent.com/knightlyj/demo/master/docs/img/shake.gif)

仔细看一下,会发现移动时角色一直在抖动,经过我一段时间(好几天)的分析,发现是因为每次Update间隔中,物理引擎的step次数不一定相同,而角色运动是基于物理引擎,这样每次Update之间,角色可能没运动,也可能经历了两次物理引擎的step,从而造成抖动.
解决方法就比较简单了,在之前代码的基础上,用两次Update之间的FixedUpdate的次数来得到物理引擎更新的次数,使得镜头追踪的运动距离与实际物理引擎的step数成正比.在上面代码的基础上,加上了一个fixedCount,大致修改如下:

```
int fixedCount = 0;
void FixedUpdate()
{
    fixedCount++;
}

void Update()
{
	//watchPoint平滑追踪角色
	Vector3 toPlayer = sight.position - watchPoint;
	//这里计算step时,用fixedCount作为乘数,两次Update间FixedUpdate次数越多,这次追踪步长也越大.
    float step = Mathf.Max(toPlayer.magnitude * fixedCount * scale, minStep);//scale用来控制追踪速度,minStep为最小追踪步长.
    watchPoint = Vector3.MoveTowards(watchPoint, sight.position, step);
	...

    fixedCount = 0; //最后要清零 
}
```


为什么不直接把镜头追踪代码写在FixedUpdate里面,而是计数,再到Update里面处理呢?查阅unity文档,可以知道物理引擎的计算是在FixedUpdate之后,如果在FixedUpdate里面追踪,那么镜头追踪角色的位置并不是实际渲染位置,实际测试也会导致一点点抖动.

## 锁定目标时的镜头
还是先看GIF

![](https://raw.githubusercontent.com/knightlyj/demo/master/docs/img/ds-lock.gif)

可以看出来,就是基于角色与目标的位置,计算出镜头期望位置和角度,再平滑运动即可,有了无锁定算法的基础,实现起来非常容易,算法图示如下:

![](https://raw.githubusercontent.com/knightlyj/demo/master/docs/img/camera-lock.gif)

红色为锁定目标的位置

再看看效果

![](https://raw.githubusercontent.com/knightlyj/demo/master/docs/img/noshake-lock.gif)

效果很好.

## 镜头碰撞
有了镜头追踪算法,再需要考虑镜头碰撞的问题.我参考了黑魂2和GTA4的镜头碰撞.

### 黑魂2的镜头碰撞.

![](https://raw.githubusercontent.com/knightlyj/demo/master/docs/img/ds-camera-collision.gif)

黑魂2的比较简单,计算出碰撞位置,然后直接把镜头放到碰撞位置.

### GTA4的镜头碰撞

![](https://raw.githubusercontent.com/knightlyj/demo/master/docs/img/gta4-camera-collision.gif)

GTA4的镜头碰撞看起来效果更好,镜头距离调整非常平滑.仔细看GIF,可以注意到镜头应该与墙壁碰撞时,镜头仅仅是缩短了与角色的距离,随着镜头继续旋转,才彻底把镜头放到墙壁前方.

### Demo的镜头碰撞
GTA4的效果实现起来有些复杂,而实际玩黑魂时,也不会太注意到镜头碰撞,故采用了黑魂2的方式,直接设置.

很多资料会说用RayCast得到碰撞位置,其实这样不够精确,镜头经常会穿模,而Unity已经提供了BoxCast,可以精确模拟镜头尺寸做碰撞计算.
效果如下.

![](https://raw.githubusercontent.com/knightlyj/demo/master/docs/img/demo-camera-collision.gif)

## 总结
实现了类似魂系列的镜头控制,遇到的主要问题是镜头抖动,最初并没有想到是物理引擎的原因,在尝试了很多方法,也整理了很多数据和经验之后,才发现是物理引擎造成的.
实际玩游戏时,完全想不到仅仅是镜头控制会这么麻烦,Devils in the details?

PS:最近玩了一会<<塞尔达-荒野之息>>,其中镜头处理就更简单,但玩起游戏来,完全注意不到这些区别- -.

PSS:因为加入了肩部射击视角,考虑到瞄准的操作精确度,最后去掉了镜头位置的平滑移动.



