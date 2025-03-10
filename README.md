# Unity Urp 17后处理解决方案

## 概述
本解决方案提供了一种便捷的方式来控制后处理流程。通过每帧将需要处理的 Pass 添加到队列中，可以灵活地管理后处理效果。在此过程中，还可以设置 Uniform 参数，以实现更精细的控制。

## 核心特性
- **Pass 队列管理**：每帧动态添加需要处理的 Pass，确保后处理流程的灵活性，这种形式可以控制Pass的顺序，可以调整案例中两个后处理效果的顺序来观察效果的差异。
- **Uniform 参数设置**：在处理过程中，可以设置 Uniform 参数，以调整后处理效果。

## 使用方法
**具体查看EfManager.cs中的实现**：
