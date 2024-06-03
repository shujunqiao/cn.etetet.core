using System;

namespace ET
{
    public static class ETCancellationTokenHelper
    {
        private static async ETTask Timeout(this ETCancellationToken self, long afterTimeCancel)
        {
            if (afterTimeCancel <= 0)
            {
                return;
            }
            
            if (self.IsCancel())
            {
                return;
            }

            // 这里用了Fiber.Instance是因为我知道有什么后果, 你们千万不能乱用这个Fiber.Instance
            await Fiber.Instance.Root.GetComponent<TimerComponent>().WaitAsync(afterTimeCancel);
            if (self.IsCancel())
            {
                return;
            }
            self.Cancel();
        }
        
        /// <summary>
        /// 增加一个canceltoken，可以用新增的canceltoken取消协程，当然也可以用老的取消
        /// </summary>
        public static async ETTask AddCancel(this ETTask task, ETCancellationToken addCancelToken)
        {
            if (addCancelToken == null)
            {
                throw new Exception("add cancel token is null");
            }
            ETCancellationToken cancelToken = await ETTaskHelper.GetCancelToken();
            
            cancelToken?.Add(addCancelToken.Cancel);
            
            await task.NewCancel(addCancelToken);
        }
        
        /// <summary>
        /// 增加一个canceltoken，可以用新增的canceltoken取消协程，当然也可以用老的取消
        /// </summary>
        public static async ETTask<T> AddCancel<T>(this ETTask<T> task, ETCancellationToken addCancelToken)
        {
            if (addCancelToken == null)
            {
                throw new Exception("add cancel token is null");
            }
            
            ETCancellationToken cancelToken = await ETTaskHelper.GetCancelToken();
            
            cancelToken?.Add(addCancelToken.Cancel);
            
            return await task.NewCancel(addCancelToken);
        }
        
        public static async ETTask Timeout(this ETTask task, ETCancellationToken cancellationToken, long afterTimeCancel)
        {
            cancellationToken.Timeout(afterTimeCancel).Coroutine();
            await AddCancel(task, cancellationToken);
        }
        
        public static async ETTask<T> Timeout<T>(this ETTask<T> task, ETCancellationToken cancellationToken, long afterTimeCancel)
        {
            cancellationToken.Timeout(afterTimeCancel).Coroutine();
            return await AddCancel(task, cancellationToken);
        }
        
        public static async ETTask Timeout(this ETTask task, long afterTimeCancel)
        {
            ETCancellationToken cancellationToken = new();
            cancellationToken.Timeout(afterTimeCancel).Coroutine();
            await AddCancel(task, cancellationToken);
        }
        
        public static async ETTask<T> Timeout<T>(this ETTask<T> task, long afterTimeCancel)
        {
            ETCancellationToken cancellationToken = new();
            cancellationToken.Timeout(afterTimeCancel).Coroutine();
            return await AddCancel(task, cancellationToken);
        }
    }
}