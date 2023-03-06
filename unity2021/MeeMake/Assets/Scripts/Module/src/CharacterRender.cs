using System.Collections.Generic;
using UnityEngine;
using LibMVCS = XTC.FMP.LIB.MVCS;

namespace MeeX.XMA
{
    public class CharacterRender
    {
        public LibMVCS.Logger logger;
        private FileCache fileCache { get; set; }
        private GameObject container { get; set; }

        private Dictionary<string, List<string>> animations = new Dictionary<string, List<string>>();

        public void Initialize(GameObject _spaceRoot)
        {
            container = new GameObject("_Character");
            container.transform.SetParent(_spaceRoot.transform);
            container.transform.localPosition = Vector3.zero;
        }

        public void Release()
        {
            GameObject.Destroy(container);
        }

        public void Setup(FileCache _fileCache)
        {
            fileCache = _fileCache;
            animations.Clear();
        }

        public void Preload(string _storyUUID, List<StoryModel.CharacterAgent> _agents)
        {
            foreach (StoryModel.CharacterAgent agent in _agents)
            {
                // preload gameobject
                if (!fileCache.HasPrefab(agent.assetCode))
                {
                    FileCache.Index index = fileCache.FindIndex(agent.assetCode);
                    if (null == index)
                        continue;

                    AssetBundle ab = fileCache.AccessAssetBundle(index.pack);
                    if (null ==ab)
                        continue;

                    GameObject go = ab.LoadAsset<GameObject>(index.file);
                    if(null==go)
                        continue;

                    fileCache.CachePrefab(agent.assetCode, go);
                }

                //cache animation
                if(!string.IsNullOrEmpty(agent.externalAnimation))
                {

                    FileCache.Index index = fileCache.FindIndex(agent.externalAnimation);
                    if (null == index)
                        continue;

                    AssetBundle ab = fileCache.AccessAssetBundle(index.pack);
                    if(null ==ab)
                        continue;

                    AnimationClip ac = ab.LoadAsset<AnimationClip>(index.file);
                    if (null == ac)
                        continue;

                    fileCache.CacheAnimationClip(agent.externalAnimation, ac);
                }
            }
        }

        public void Render(StoryModel.Story _story)
        {
            foreach (StoryModel.CharacterAgent agent in _story.characters)
            {
                GameObject go = fileCache.AccessPrefab(agent.assetCode);
                if (null == go)
                    continue;

                Vector3 position = new Vector3((float)agent.px, (float)agent.py, (float)agent.pz);
                Quaternion rotation = Quaternion.Euler((float)agent.rx, (float)agent.ry, (float)agent.rz);
                GameObject clone = GameObject.Instantiate(go, position, rotation, container.transform);
                clone.name = agent.uuid;
                clone.transform.localScale = new Vector3((float)agent.sx, (float)agent.sx, (float)agent.sz);
                clone.SetActive(agent.visible);

                RenderUtility.Agent.AssignMaterial(clone.transform, agent.materials);

                Animator animator = clone.GetComponent<Animator>();
                if (null == animator)
                    continue;

                animations[agent.uuid] = new List<string>();
                foreach (AnimatorControllerParameter acp in animator.parameters)
                {
                    animations[agent.uuid].Add(acp.name);
                }
                var rac = animator.runtimeAnimatorController;
                var aoc = new AnimatorOverrideController(rac);
                animator.runtimeAnimatorController = aoc;

                if(!string.IsNullOrEmpty(agent.externalAnimation)) {
                    var ac = fileCache.AccessAnimationClip(agent.externalAnimation);
                    if(null != ac)
                    {
                        aoc["external"] = ac;
                        animator.SetTrigger("_external_");
                    }
                    else
                    {
                        logger.Error("animation {0} not found", agent.externalAnimation);
                    }
                } else if(!string.IsNullOrEmpty(agent.internalAnimation)) {
                    PlayInternalAnimation(agent.uuid, agent.internalAnimation);
                }
            }
        }

        public Transform FindCharacter(string _uuid)
        {
            return container.transform.Find(_uuid);
        }

        public void PlayInternalAnimation(string _characterUUID, string _anim)
        {
            List<string> anims = new List<string>();
            if (!animations.TryGetValue(_characterUUID, out anims))
                return;

            string[] vars = _anim.Split('.');
            if (0 == vars.Length)
                return;

            int index = 0;
            if (!int.TryParse(vars[0], out index))
                return;

            if (index >= anims.Count)
                return;

            Transform target = container.transform.Find(_characterUUID);
            if (null == target)
                return;

            Animator animator = target.GetComponent<Animator>();
            if (null == animator)
                return;

            animator.SetTrigger(anims[index]);
        }
    }
}
