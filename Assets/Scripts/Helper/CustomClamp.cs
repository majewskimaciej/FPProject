namespace Helper
{
    public static class CustomClamp
    {
        public static float Clamp(float currentDegrees, float jumpStartDegrees, float limit)
        {
            var center = (jumpStartDegrees + 180) % 360;
            var lBarrier = jumpStartDegrees - limit;
            var rBarrier = jumpStartDegrees + limit;
            
            if (currentDegrees <= rBarrier && currentDegrees >= lBarrier)
            {
                return currentDegrees;
            }

            if (jumpStartDegrees - limit < 0)
            {
                var lBarrierEdge = 360 + jumpStartDegrees - limit;
                var rBarrierEdge = jumpStartDegrees + limit;
                
                return CheckOuterEdges(currentDegrees, rBarrierEdge, lBarrierEdge, center);
            }

            if (jumpStartDegrees + limit > 360)
            {
                var lBarrierEdge = jumpStartDegrees - limit;
                var rBarrierEdge = jumpStartDegrees + limit - 360;

                return CheckOuterEdges(currentDegrees, rBarrierEdge, lBarrierEdge, center);
            }

            return CheckInsideEdges(currentDegrees, rBarrier, lBarrier, center, jumpStartDegrees);
        }

        private static float CheckOuterEdges(float currentDegrees, float rBarrierEdge, float lBarrierEdge, float center)
        {
            if (currentDegrees <= rBarrierEdge || currentDegrees >= lBarrierEdge)
            {
                return currentDegrees;
            }

            if (currentDegrees < lBarrierEdge && currentDegrees > center)
            {
                return lBarrierEdge;
            }

            if (currentDegrees > rBarrierEdge && currentDegrees < center)
            {
                return rBarrierEdge;
            }

            return currentDegrees;
        }

        private static float CheckInsideEdges(float currentDegrees, float rBarrier, float lBarrier, float center,
            float jumpStartDegrees) => jumpStartDegrees switch
        {
            >= 90 and < 270 when currentDegrees < lBarrier => lBarrier,
            >= 90 and < 270 when currentDegrees > rBarrier => rBarrier,
            > 45 and < 90 when currentDegrees < lBarrier => lBarrier,
            > 45 and < 90 when currentDegrees > center => lBarrier,
            > 45 and < 90 when currentDegrees > rBarrier => rBarrier,
            >= 270 and < 315 when currentDegrees > rBarrier => rBarrier,
            >= 270 and < 315 when currentDegrees < center => rBarrier,
            >= 270 and < 315 when currentDegrees < lBarrier => lBarrier,
            _ => currentDegrees
        };
    }
}
