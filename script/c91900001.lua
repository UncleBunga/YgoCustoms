--Smalls
local s,id=GetID()
function s.initial_effect(c)
  --discard, special summon banished Frog/Toad
  local e1=Effect.CreateEffect(c)
  e1:SetDescription(aux.Stringid(id,0))
  e1:SetCategory(CATEGORY_SPECIAL_SUMMON)
  e1:SetType(EFFECT_TYPE_IGNITION)
  e1:SetRange(LOCATION_HAND)
  e1:SetProperty(EFFECT_FLAG_CARD_TARGET)
  e1:SetCost(s.discost)
  e1:SetTarget(s.target)
  e1:SetOperation(s.operation)
  c:RegisterEffect(e1)

  --destruction replacement
  local e2=Effect.CreateEffect(c)
  e2:SetType(EFFECT_TYPE_FIELD+EFFECT_TYPE_CONTINUOUS)
  e2:SetCode(EFFECT_DESTROY_REPLACE)
  e2:SetRange(LOCATION_GRAVE)
  e2:SetTarget(s.reptg)
  e2:SetValue(s.repval)
  e2:SetOperation(s.repop)
  c:RegisterEffect(e2)
end

-- Helper function to check for "Frog" archetype OR specific "Toad" IDs
function s.frog_toad_filter(c)
  -- IDs: 68638985 (Slime Toad), 20663556 (Substitoad), 23408872 (Tradetoad), 1357146 (Ronintoadin), 90809975 (Toadally Awesome)
  return c:IsSetCard(0x12) or c:IsCode(68638985, 20663556, 23408872, 1357146, 90809975)
end

function s.discost(e,tp,eg,ep,ev,re,r,rp,chk)
  local c=e:GetHandler()
  if chk==0 then return c:IsDiscardable() end
  Duel.SendtoGrave(c,REASON_COST+REASON_DISCARD)
end

function s.spfilter(c,e,tp)
  return c:IsFaceup() and s.frog_toad_filter(c)
    and c:IsCanBeSpecialSummoned(e,0,tp,false,false)
end

function s.target(e,tp,eg,ep,ev,re,r,rp,chk,chkc)
  if chkc then return chkc:IsLocation(LOCATION_REMOVED) and s.spfilter(chkc,e,tp) end
  if chk==0 then 
    return Duel.GetLocationCount(tp,LOCATION_MZONE)>0 
      and Duel.IsExistingTarget(s.spfilter,tp,LOCATION_REMOVED,LOCATION_REMOVED,1,nil,e,tp) 
  end
  Duel.Hint(HINT_SELECTMSG,tp,HINTMSG_SPSUMMON)
  local g=Duel.SelectTarget(tp,s.spfilter,tp,LOCATION_REMOVED,LOCATION_REMOVED,1,1,nil,e,tp)
  Duel.SetOperationInfo(0,CATEGORY_SPECIAL_SUMMON,g,1,0,0)
end

function s.operation(e,tp,eg,ep,ev,re,r,rp)
  local tc=Duel.GetFirstTarget()
  if tc and tc:IsRelateToEffect(e) then
    Duel.SpecialSummon(tc,0,tp,tp,false,false,POS_FACEUP)
  end
end

-- Updated to protect Frog/Toads instead of the previous Level/Rank 2 check
function s.repfilter(c,tp)
  return c:IsFaceup() and c:IsControler(tp) and c:IsLocation(LOCATION_MZONE)
    and s.frog_toad_filter(c)
    and not c:IsReason(REASON_REPLACE) and c:IsReason(REASON_EFFECT+REASON_BATTLE)
end

function s.reptg(e,tp,eg,ep,ev,re,r,rp,chk)
  local c=e:GetHandler()
  if chk==0 then return c:IsAbleToRemove() and eg:IsExists(s.repfilter,1,nil,tp) end
  return Duel.SelectEffectYesNo(tp,c,96)
end

function s.repval(e,c)
  return s.repfilter(c,e:GetHandlerPlayer())
end

function s.repop(e,tp,eg,ep,ev,re,r,rp)
  Duel.Remove(e:GetHandler(),POS_FACEUP,REASON_EFFECT+REASON_REPLACE)
end