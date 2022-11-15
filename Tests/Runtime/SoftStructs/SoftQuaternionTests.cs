using NUnit.Framework;

namespace GameLibrary.Mathematics.Tests
{
    public class SoftQuaternionTests
    {
        [Test]
        public static void DotProductShouldBeOneOrNegativeOneOnSameRotations()
        {
            // Arrange
            SoftQuaternion rotation180 = new SoftQuaternion(SoftFloat.One, SoftFloat.Zero, SoftFloat.Zero, SoftFloat.Zero);
            SoftQuaternion rotationMinus180 = new SoftQuaternion(-SoftFloat.One, SoftFloat.Zero, SoftFloat.Zero, SoftFloat.Zero);
            
            // Act
            SoftFloat negativeDot = SoftQuaternion.Dot(rotation180, rotationMinus180);
            SoftFloat dot = SoftQuaternion.Dot(rotation180, rotation180);
            
            // Assert
            Assert.IsTrue(SoftMath.ApproximatelyEqual(negativeDot, SoftFloat.MinusOne));
            Assert.IsTrue(SoftMath.ApproximatelyEqual(dot, SoftFloat.One));
        }

        [Test]
        public static void MultiplicationWithVectorShouldRotateVector()
        {
            // Arrange
            SoftVector3 point = SoftVector3.Up;
            SoftQuaternion rotation180 = new SoftQuaternion(SoftFloat.One, SoftFloat.Zero, SoftFloat.Zero, SoftFloat.Zero);
            
            // Act
            SoftVector3 transformed = rotation180 * point;
            
            // Assert
            Assert.IsTrue(transformed == SoftVector3.Down);
        }
        
        [Test]
        public static void MultiplicationWithVectorShouldRotateAndScaleVector()
        {
            // Arrange
            SoftVector3 point = SoftVector3.Up;
            SoftQuaternion rotation180 = new SoftQuaternion(SoftFloat.One, SoftFloat.Zero, SoftFloat.Zero, SoftFloat.Zero);
            SoftQuaternion scaling9 = new SoftQuaternion(SoftFloat.Zero, SoftFloat.Zero, SoftFloat.Zero, (SoftFloat)3f);

            // Act
            SoftVector3 transformed = scaling9 * rotation180 * point;
            
            // Assert
            Assert.IsTrue(transformed == SoftVector3.Down * (SoftFloat)9f);
        }
    }
}